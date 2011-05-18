using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Floe.Net;

namespace Floe.UI
{
	public class NicknameList : KeyedCollection<string, NicknameItem>, INotifyCollectionChanged
	{
		public NicknameList()
			: base(StringComparer.OrdinalIgnoreCase)
		{
		}

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public void AddRange(IEnumerable<string> nicks)
		{
			var items = nicks.Select((n) => new NicknameItem(n)).ToList();
			foreach (var item in items)
			{
				this.Add(item);
			}
		}

		public void Add(string nick)
		{
			this.Add(new NicknameItem(nick));
		}

		public void ChangeNick(string oldNick, string newNick)
		{
			var item = this[oldNick];
			if (item != null)
			{
				var idx = this.IndexOf(item);
				this.ChangeItemKey(item, newNick);
				item.Nickname = newNick;
				this.RefreshItem(idx);
			}
		}

		public void ProcessMode(IrcChannelMode mode)
		{
			var mask = ChannelLevel.Normal;
			switch (mode.Mode)
			{
				case 'o':
					mask = ChannelLevel.Op;
					break;
				case 'h':
					mask = ChannelLevel.HalfOp;
					break;
				case 'v':
					mask = ChannelLevel.Voice;
					break;
			}

			if (mask != ChannelLevel.Normal && this.Contains(mode.Parameter))
			{
				var item = this[mode.Parameter];
				if (item != null)
				{
					item.Level = mode.Set ? item.Level | mask : item.Level & ~mask;
					var idx = this.IndexOf(item);
					this.RefreshItem(idx);
				}
			}
		}

		protected override string GetKeyForItem(NicknameItem item)
		{
			return item.Nickname;
		}

		protected override void SetItem(int index, NicknameItem item)
		{
			var oldItem = this[index];
			base.SetItem(index, item);
			this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem));
		}

		protected override void InsertItem(int index, NicknameItem item)
		{
			base.InsertItem(index, item);
			this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
		}

		protected override void ClearItems()
		{
			base.ClearItems();
			this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		protected override void RemoveItem(int index)
		{
			var item = this[index];
			base.RemoveItem(index);
			this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));	
		}

		private void RefreshItem(int idx)
		{
			var item = this[idx];
			this.SetItem(idx, new NicknameItem(""));
			this.SetItem(idx, item);
		}

		private void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
		{
			var handler = this.CollectionChanged;
			if (handler != null)
			{
				handler(this, args);
			}
		}
	}
}
