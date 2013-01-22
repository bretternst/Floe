using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Floe.Net;

namespace Floe.UI
{
	public partial class ChatControl : ChatPage
	{
		private string[] _nickCandidates;
		private NicknameList _nickList;

		public NicknameList Nicknames
		{
			get { return _nickList; }
		}
		
		private string GetNickWithLevel(string nick)
		{
			return this.IsChannel && _nickList.Contains(nick) ? _nickList[nick].ToString() : nick;
		}

		private string GetNickWithoutLevel(string nick)
		{
			return (nick.Length > 1 && (nick[0] == '@' || nick[0] == '+' || nick[0] == '%')) ? nick.Substring(1) : nick;
		}

		private void DoNickCompletion()
		{
			int start = 0, end = 0;
			if (txtInput.Text.Length > 0)
			{
				start = Math.Max(0, txtInput.CaretIndex - 1);
				end = start < txtInput.Text.Length ? start + 1 : start;

				while (start >= 0 && NicknameItem.IsNickChar(txtInput.Text[start]))
				{
					start--;
				}
				start++;

				while (end < txtInput.Text.Length && NicknameItem.IsNickChar(txtInput.Text[end]))
				{
					end++;
				}
			}
			else
			{
				start = end = 0;
			}

			if (end - start > 0)
			{
				string nickPart = txtInput.Text.Substring(start, end - start);
				string nextNick = null;
				if (_nickCandidates == null)
				{
					_nickCandidates = (from n in this.Nicknames
									   where n.Nickname.StartsWith(nickPart, StringComparison.InvariantCultureIgnoreCase)
									   orderby n.Nickname.ToLowerInvariant()
									   select n.Nickname).ToArray();
					if (_nickCandidates.Length > 0)
					{
						nextNick = _nickCandidates[0];
					}
				}

				for (int i = 0; i < _nickCandidates.Length; i++)
				{
					if (string.Compare(_nickCandidates[i], nickPart, StringComparison.InvariantCulture) == 0)
					{
						nextNick = i < _nickCandidates.Length - 1 ? _nickCandidates[i + 1] : _nickCandidates[0];
						break;
					}
				}

				var keepNickCandidates = _nickCandidates;
				if (nextNick != null)
				{
					txtInput.Text = txtInput.Text.Substring(0, start) + nextNick + txtInput.Text.Substring(end);
					txtInput.CaretIndex = start + nextNick.Length;
				}
				_nickCandidates = keepNickCandidates;
			}
		}
	}
}
