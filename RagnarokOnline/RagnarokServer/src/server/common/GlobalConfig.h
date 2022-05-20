#pragma once

#include <string_view>

class GlobalConfig
{
public:
	static const auto& dataPath()        { return _dataPath; }
	static const auto& certificatePath() { return _certificatePath; }
	static const auto& listenPort()      { return _listenPort; }

	static constexpr auto& serverCertificate()                 { return _serverCertificate; }
	static constexpr auto& certificatePrivateKey()             { return _certificatePrivateKey; }
	static constexpr auto& certificationAuthorityCertificate() { return _certificationAuthorityCertificate; }
	static constexpr auto& diffHelmanFile()                    { return _diffHelmanFile; }
	
private:
	friend int main(int argc, char* argv[]);
	static void init(int argc, char* argv[]);

	static constexpr std::string_view _serverCertificate = "certificate.crt";
	static constexpr std::string_view _certificatePrivateKey = "private.key";
	static constexpr std::string_view _certificationAuthorityCertificate = "ca_bundle.crt";
	static constexpr std::string_view _diffHelmanFile = "dh1024.pem";

	static inline std::string_view _dataPath;
	static inline std::string_view _certificatePath;
	static inline uint16_t _listenPort = 0;
};