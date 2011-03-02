#pragma once
#include "Stdafx.h"
#include "Common.h"
#include "AudioDevice.h"

namespace Floe
{
	namespace Audio
	{
		using namespace System::Threading;
		using namespace System::Threading::Tasks;

		public ref class AudioClient abstract
		{
		private:
			IAudioClient *m_iac;
			WaveFormat ^m_format;
			Task ^m_task;
			ManualResetEvent ^m_cancelEvent;
			AutoResetEvent ^m_bufferEvent;
			int m_bufferSize;

		public:
			AudioClient(AudioDevice^ device);
			void Start();
			void Stop();

		protected:
			property IAudioClient *Client
			{
				IAudioClient *get()
				{
					return m_iac;
				}
			}

			property WaveFormat ^Format
			{
				WaveFormat ^get()
				{
					return m_format;
				}
			}

			property int BufferSizeInFrames
			{
				int get()
				{
					return m_bufferSize;
				}
			}

			property int BufferSizeInBytes
			{
				int get()
				{
					return m_bufferSize * m_format->FrameSize;
				}
			}

			property int FrameSize
			{
				int get()
				{
					return m_format->FrameSize;
				}
			}

			property WaitHandle ^BufferHandle
			{
				WaitHandle ^get()
				{
					return m_bufferEvent;
				}
			}

			property WaitHandle ^CancelHandle
			{
				WaitHandle ^get()
				{
					return m_cancelEvent;
				}
			}

			virtual void Loop() abstract;

		private:
			~AudioClient();
			!AudioClient();
		};
	}
}
