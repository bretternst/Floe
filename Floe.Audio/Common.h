#pragma once
#include "Stdafx.h"

using System::String;
using System::Exception;

namespace Floe
{
	namespace Audio
	{
		public ref class AudioException : Exception
		{
		public:
			AudioException(String^ message)
				: Exception(message)
			{
			}
		};

		inline void ThrowOnFailure(HRESULT hr)
		{
			if(hr != 0)
			{
				throw gcnew AudioException(System::String::Format("Audio error: {0}", hr.ToString("X")));
			}
		}
	}
}