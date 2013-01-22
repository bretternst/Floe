#include "Stdafx.h"
#include "RawInput.h"

namespace Floe
{
	namespace Interop
	{
		void RawInput::Initialize(Window ^window)
		{
			RAWINPUTDEVICE devices[2];
			devices[0].usUsagePage = 1;
			devices[0].dwFlags = RIDEV_INPUTSINK;
			devices[0].hwndTarget = (HWND)(void*)(gcnew WindowInteropHelper(window))->Handle;
			devices[1] = devices[0];
			devices[0].usUsage = 6;
			devices[1].usUsage = 2;
			RegisterRawInputDevices(&devices[0], 2, sizeof(RAWINPUTDEVICE));
		}

		void RawInput::HandleInput(IntPtr lParam)
		{
			RAWINPUT input;
			int size;
			GetRawInputData((HRAWINPUT)(void*)lParam, RID_INPUT, 0, (PUINT)&size, sizeof(RAWINPUTHEADER));
			if(size > sizeof(RAWINPUT))
			{
				return;
			}
			GetRawInputData((HRAWINPUT)(void*)lParam, RID_INPUT, &input, (PUINT)&size, sizeof(RAWINPUTHEADER));
			InputButton key = (InputButton)0;
			bool isKeyDown = false;
			switch(input.header.dwType)
			{
			case RIM_TYPEKEYBOARD:
				 key = (InputButton)input.data.keyboard.VKey;
				 switch(key)
				 {
				 case InputButton::ShiftKey:
					 key = (InputButton)MapVirtualKey(input.data.keyboard.MakeCode, MAPVK_VSC_TO_VK_EX);
					 break;
				 case InputButton::Menu:
					 key = (input.data.keyboard.Flags & RI_KEY_E0) > 0 ? InputButton::RMenu : InputButton::LMenu;
					 break;
				 case InputButton::ControlKey:
					 key = (input.data.keyboard.Flags & RI_KEY_E0) > 0 ? InputButton::RControlKey : InputButton::LControlKey;
					 break;
				 case InputButton::Enter:
					 key = (input.data.keyboard.Flags & RI_KEY_E0) > 0 ? InputButton::Separator : InputButton::Enter;
					 break;
				 }
				 isKeyDown = (input.data.keyboard.Flags & RI_KEY_BREAK) == 0;
				break;
			case RIM_TYPEMOUSE:
				if((input.data.mouse.usButtonFlags & RI_MOUSE_LEFT_BUTTON_DOWN) > 0)
				{
					key = InputButton::LMouseButton;
					isKeyDown = true;
				}
				else if ((input.data.mouse.usButtonFlags & RI_MOUSE_LEFT_BUTTON_UP) > 0)
				{
					key = InputButton::LMouseButton;
				}
				else if((input.data.mouse.usButtonFlags & RI_MOUSE_RIGHT_BUTTON_DOWN) > 0)
				{
					key = InputButton::RMouseButton;
					isKeyDown = true;
				}
				else if ((input.data.mouse.usButtonFlags & RI_MOUSE_RIGHT_BUTTON_UP) > 0)
				{
					key = InputButton::RMouseButton;
				}
				else if((input.data.mouse.usButtonFlags & RI_MOUSE_MIDDLE_BUTTON_DOWN) > 0)
				{
					key = InputButton::MMouseButton;
					isKeyDown = true;
				}
				else if ((input.data.mouse.usButtonFlags & RI_MOUSE_MIDDLE_BUTTON_UP) > 0)
				{
					key = InputButton::MMouseButton;
				}
				else if((input.data.mouse.usButtonFlags & RI_MOUSE_BUTTON_4_DOWN) > 0)
				{
					key = InputButton::MouseButton4;
					isKeyDown = true;
				}
				else if ((input.data.mouse.usButtonFlags & RI_MOUSE_BUTTON_4_UP) > 0)
				{
					key = InputButton::MouseButton4;
				}
				else if((input.data.mouse.usButtonFlags & RI_MOUSE_BUTTON_5_DOWN) > 0)
				{
					key = InputButton::MouseButton5;
					isKeyDown = true;
				}
				else if ((input.data.mouse.usButtonFlags & RI_MOUSE_BUTTON_5_UP) > 0)
				{
					key = InputButton::MouseButton5;
				}
				break;
			}

			if(m_eventArgs == nullptr)
			{
				m_eventArgs = gcnew RawInputEventArgs();
			}
			m_eventArgs->Handled = false;

			if((int)key > 0)
			{
				m_eventArgs->Button = key;
				if(isKeyDown)
				{
					ButtonDown(nullptr, m_eventArgs);
				}
				else
				{
					ButtonUp(nullptr, m_eventArgs);
				}
			}

			if(!m_eventArgs->Handled)
			{
				PRAWINPUT pInput = &input;
				DefRawInputProc(&pInput, 1, sizeof(RAWINPUT));
			}
		}
	}
}
