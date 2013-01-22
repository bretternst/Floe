#pragma once
#include "Stdafx.h"
#include "InputButton.h"

namespace Floe
{
	namespace Interop
	{
		using System::Windows::Window;
		using System::Windows::Interop::WindowInteropHelper;
		using System::IntPtr;

		public ref class RawInputEventArgs : System::EventArgs
		{
		private:
			InputButton m_button;
			bool m_handled;

		public:
			property InputButton Button
			{
				InputButton get()
				{
					return m_button;
				}
			internal:
				void set(InputButton button)
				{
					m_button = button;
				}
			}

			property bool Handled
			{
				bool get()
				{
					return m_handled;
				}
				void set(bool handled)
				{
					m_handled = handled;
				}
			}

		internal:
			RawInputEventArgs() {}
		};

		public ref class RawInput
		{
		private:
			static RawInputEventArgs ^m_eventArgs;

		public:
			static void Initialize(Window ^window);
			static void HandleInput(IntPtr lParam);

			static event System::EventHandler<RawInputEventArgs^> ^ButtonDown;
			static event System::EventHandler<RawInputEventArgs^> ^ButtonUp;
		};
	}
}
