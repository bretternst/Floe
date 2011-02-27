#pragma once
#include "Stdafx.h"
#include "Common.h"

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
			WAVEFORMATEX *m_format;
			Task ^m_task;
			ManualResetEvent ^m_cancelEvent;
			AutoResetEvent ^m_bufferEvent;
			int m_bufferSize;

		public:
			AudioClient();
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

			property WAVEFORMATEX *Format
			{
				WAVEFORMATEX *get()
				{
					return m_format;
				}
			}

			property int BufferSize
			{
				int get()
				{
					return m_bufferSize;
				}
			}

			property int BlockSize
			{
				int get()
				{
					return m_format->nBlockAlign;
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
