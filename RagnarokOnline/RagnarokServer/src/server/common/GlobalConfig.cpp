#include "GlobalConfig.h"

#include <charconv>
#include <exception>
#include <filesystem>

namespace fs = std::filesystem;

void GlobalConfig::init(int argc, char * argv[])
{
	if (argc != 4)
		throw std::runtime_error("Invalid number of commandline arguments");

	union
	{
		struct
		{
			uint32_t _dataPath : 1;
			uint32_t _certificatePath : 1;
			uint32_t _listenPort : 1;
		};
		uint32_t _raw;
	} argumentFoundMask;

	argumentFoundMask._raw = 0;
	argumentFoundMask._dataPath = 1;
	argumentFoundMask._certificatePath = 1;
	argumentFoundMask._listenPort = 1;

	static constexpr std::string_view dataPath        = "dataPath=";
	static constexpr std::string_view certificatePath = "certificatePath=";
	static constexpr std::string_view listenPort      = "listenPort=";

	for (int i = 1; i < argc; i++)
	{
		std::string_view arg(argv[i]);

		if (arg.compare(0, dataPath.length(), dataPath) == 0)
		{
			GlobalConfig::_dataPath = arg.substr(dataPath.length(), arg.length() - dataPath.length());

			if(!fs::exists(GlobalConfig::_dataPath))
				throw std::runtime_error("Invalid dataPath or dataPath not found");

			argumentFoundMask._dataPath = 0;
		}
		else if (arg.compare(0, certificatePath.length(), certificatePath) == 0)
		{
			GlobalConfig::_certificatePath = arg.substr(certificatePath.length(), arg.length() - certificatePath.length());

			if (!fs::exists(_certificatePath))
				throw std::runtime_error("Invalid certificatePath or certificatePath not found");

			argumentFoundMask._certificatePath = 0;
		}
		else if (arg.compare(0, listenPort.length(), listenPort) == 0)
		{
			auto subStr = arg.substr(listenPort.length(), arg.length() - listenPort.length());

			auto result = std::from_chars(subStr.data(), subStr.data() + subStr.length(), GlobalConfig::_listenPort);

			if(result.ec != std::errc())
				throw std::runtime_error("Invalid listen port specified");

			argumentFoundMask._listenPort = 0;
		}
	}

	if (argumentFoundMask._raw != 0)
		throw std::runtime_error("Not all required arguments were passed");
}
