#pragma once

#include "MapData.h"

#include <fstream>
#include <stdexcept>

namespace map
{
	MapData::MapData(std::ifstream& file)
	{
		//open before call so each component can read its section
		std::ifstream file(filename, std::ios::in | std::ios::binary);

		if (!file.is_open())
			assert(false, filePath);
		
		/*size = file.tellg();
		memblock = new char[size];
		file.seekg(0, ios::beg);
		file.read(memblock, size);
		file.close();*/
	}
}