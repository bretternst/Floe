#pragma once
#include "Stdafx.h"
#include "WaveFormat.h"

using System::String;
using System::Exception;

namespace Floe
{
	namespace Interop
	{
		public ref class InteropException : Exception
		{
		public:
			InteropException(String^ message)
				: Exception(message)
			{
			}
		};

		inline void ThrowOnFailure(HRESULT hr)
		{
			if(hr != 0)
			{
				throw gcnew InteropException(System::String::Format("System error: {0}", hr.ToString("X")));
			}
		}

		inline void ThrowOnZero(HANDLE handle)
		{
			if(handle == 0)
			{
				throw gcnew InteropException("A null handle was returned.");
			}
		}
	}
}