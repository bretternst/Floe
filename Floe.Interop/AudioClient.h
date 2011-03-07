#pragma once
#include "Stdafx.h"
#include "Common.h"
#include "AudioDevice.h"

namespace Floe
{
	namespace Interop
	{
		using namespace System::Threading;
		using namespace System::Threading::Tasks;

		public ref class AudioClient abstract
		{
		private:
			IAudioClient *m_iac;
			ISimpleAudioVolume *m_isav;
			WaveFormat ^m_format;
			Task ^m_task;
			ManualResetEvent ^m_cancelEvent;
			AutoResetEvent ^m_bufferEvent;
			int m_bufferSize;

		public:
			AudioClient(AudioDevice^ device);
			void Start();
			void Stop();

			property float Volume
			{
				float get()
				{
					float vol;
					m_isav->GetMasterVolume(&vol);
					return vol;
				}
				void set(float vol)
				{
					m_isav->SetMasterVolume(vol, 0);
				}
			}

			property bool IsMuted
			{
				bool get()
				{
					BOOL mute;
					m_isav->GetMute(&mute);
					return mute != FALSE;
				}
				void set(bool mute)
				{
					m_isav->SetMute(mute ? TRUE : FALSE, 0);
				}
			}

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
