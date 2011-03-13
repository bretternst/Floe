using System;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;

using Floe.Net;
using Floe.Audio;

namespace Floe.UI
{
	public partial class VoiceControl : UserControl, IDisposable
	{
		public static readonly DependencyProperty IsChattingProperty = DependencyProperty.Register(
			"IsChatting", typeof(bool), typeof(VoiceControl));
		public bool IsChatting
		{
			get { return (bool)this.GetValue(IsChattingProperty); }
			set { this.SetValue(IsChattingProperty, value); }
		}

		public static readonly DependencyProperty IsTransmittingProperty = DependencyProperty.Register(
			"IsTransmitting", typeof(bool), typeof(VoiceControl));
		public bool IsTransmitting
		{
			get { return (bool)this.GetValue(IsTransmittingProperty); }
			set { this.SetValue(IsTransmittingProperty, value); }
		}

		public static readonly DependencyProperty IsVoiceChatProperty = DependencyProperty.RegisterAttached(
			"IsVoiceChat", typeof(bool), typeof(VoiceControl));
		public static void SetIsVoiceChat(DependencyObject obj, bool value)
		{
			obj.SetValue(IsVoiceChatProperty, value);
		}
		public static bool GetIsVoiceChat(DependencyObject obj)
		{
			return (bool)obj.GetValue(IsVoiceChatProperty);
		}

		public static readonly DependencyProperty IsMutedProperty = DependencyProperty.RegisterAttached(
			"IsMuted", typeof(bool), typeof(VoiceControl));
		public static void SetIsMuted(DependencyObject obj, bool value)
		{
			obj.SetValue(IsMutedProperty, value);
		}
		public static bool GetIsMuted(DependencyObject obj)
		{
			return (bool)obj.GetValue(IsMutedProperty);
		}

		public static readonly DependencyProperty IsTalkingProperty = DependencyProperty.RegisterAttached(
			"IsTalking", typeof(bool), typeof(VoiceControl));
		public static void SetIsTalking(DependencyObject obj, bool value)
		{
			obj.SetValue(IsTalkingProperty, value);
		}
		public static bool GetIsTalking(DependencyObject obj)
		{
			return (bool)obj.GetValue(IsTalkingProperty);
		}
	}
}
