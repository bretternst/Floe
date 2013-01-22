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

		public ref class InteropErrorEventArgs : System::EventArgs
		{
		private:
			Exception ^m_exception;

		internal:
			InteropErrorEventArgs(Exception ^exception) : m_exception(exception)
			{
			}

		public:
			property Exception ^Exception
			{
				System::Exception ^get()
				{
					return m_exception;
				}
			}
		};

		inline void ThrowOnFailure(HRESULT hr)
		{
			switch(hr)
			{
			case MMSYSERR_ALLOCATED:
				throw gcnew InteropException("The multimedia device is already in use.");
			}

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