#include <server/ServerContext.h>
#include <server/lobby/ServerCommand.h>
#include <server/common/GlobalConfig.h>

#include <sdk/Tick.h>
#include <sdk/log/Log.h>
#include <sdk/log/LogController.h>

#include <chrono>
#include <memory>
#include <future>

#include <server/network/Packets.h> 

using namespace std::chrono;

int main(int argc, char* argv[])
{	
	GlobalConfig::init(argc, argv);
	LogControllerWrapper logController;
	(*logController).initThreadLocalLogger();

	while (true)
	{
		std::promise<ServerCommand> commandPromise;
		std::future<ServerCommand> commandFuture = commandPromise.get_future();

		(*logController).setThreadLocalTimestamp();
		Tick::_bootValue = duration_cast<milliseconds>(steady_clock::now().time_since_epoch());
		Log::info("Server boot started", "main");

		ServerContext serverContext(commandPromise, *logController);
		
		switch (commandFuture.get())
		{
		case ServerCommand::Reboot:
		{
			Log::info("Server will re-boot", "main");
			continue;
		}
		case ServerCommand::Shutdown:
			Log::info("Server is shutting down", "main");
			break;
		}
	}

	return 0;
}