using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.UI;

namespace UIModels.MultipurposeModels
{
    public abstract class RotaryListModel<T> : ModelBase
    {
        public class ListItem<T> : IListItem
        {
            public event EventHandler CaptionChanged;
            public event EventHandler FocusChanged;

            private readonly EventHandler clickHandler;
            private readonly ONBSSyncContext syncContext;
            private readonly List<ListItem<T>> list;

            private bool focused;
            private string caption;

            public T Value
            {
                get;
                set;
            }

            public string Caption
            {
                get
                {
                    return caption;
                }
                set
                {
                    caption = value;
                    var handler = CaptionChanged;
                    if (handler != null)
                        handler(this, null);
                }
            }

            public bool Focused
            {
                get
                {
                    return focused;
                }
                set
                {
                    if (value != focused)
                    {
                        focused = value;
                        var handler = FocusChanged;
                        if (handler != null)
                            handler(this, null);

                        if (focused)
                            UnfocusOther();
                    }
                }
            }

            private void UnfocusOther()
            {
                foreach(var other in list.Where(l => l != this && l.Focused))
                {
                    other.Focused = false;
                }
            }

            public ListItem(T value, string caption, EventHandler clickHandler, ONBSSyncContext syncContext, List<ListItem<T>> list)
            {
                Value = value;
                Caption = caption;
                this.clickHandler = clickHandler;
                this.syncContext = syncContext;
                this.list = list;
            }

            public void Click()
            {
                if (clickHandler != null)
                    syncContext.Post(o => clickHandler(this, null), null, "RotaryListModel item click");
            }

            public static void PrepareItem(ONBSSyncContext syncContext, ref List<ListItem<T>> list, T value, EventHandler handler, string caption = null)
            {
                if (list == null)
                    list = new List<ListItem<T>>();

                list.Add(new ListItem<T>(value, caption ?? (value != null ? value.ToString() : null), handler, syncContext, list));
            }

            public static void PrepareItems(ONBSSyncContext syncContext, ref List<ListItem<T>> list, IEnumerable<T> values, EventHandler handler, Func<T, string> captionFunc)
            {
                if (list == null)
                    list = new List<ListItem<T>>();

                foreach(var value in values)
                {
                    list.Add(new ListItem<T>(value, captionFunc(value), handler, syncContext, list));
                }
            }
        }

        private readonly int itemsPerPage;
        private readonly int desiredFocusIndex;
        private readonly string itemsSourceName;

        private int skip;

        private LinkedList<ListItem<T>> linkedList;

        protected int SelectedIndexAbsolute
        {
            get
            {
                var focusedNode = GetFocusedNode();
                if (focusedNode != null)
                {
                    return skip * itemsPerPage + Array.IndexOf(linkedList.ToArray(), focusedNode.Value);
                }
                else 
                    return -1;               
            }
        }

        public RotaryListModel(string viewName, IHostController hc, MappedPage pageDescriptor, string itemsSourceName, int itemsPerPage, int focusedIndex = 0)
            : base(viewName, hc, pageDescriptor)
        {
            this.itemsPerPage = itemsPerPage;
            this.itemsSourceName = itemsSourceName;

            if (focusedIndex >= 0)
            {
                skip = focusedIndex / itemsPerPage;
                desiredFocusIndex = focusedIndex - (skip * itemsPerPage);
            }

            SetProperty("items_source_prop_name", itemsSourceName);
            SetProperty(ModelNames.ButtonPrevLabel, "⇑");
            SetProperty(ModelNames.ButtonNextLabel, "⇓ ");
        }

        protected override void Initialize()
        {
            var itemsCount = QueryItems();

            if (itemsCount > 0)
            {
                if (itemsCount > desiredFocusIndex)
                    linkedList.Skip(desiredFocusIndex).First().Focused = true;
                else
                    linkedList.First().Focused = true;
            }
            else
            {
                skip = 0;
                itemsCount = QueryItems();

                if (itemsCount > 0)
                    linkedList.First().Focused = true;
            }

            PushList();

            base.Initialize();
        }

        protected abstract IList<ListItem<T>> QueryItems(int skip, int take);

        private int QueryItems()
        {
            var items = QueryItems(skip, itemsPerPage);

            if (items != null)
            {
                linkedList = new LinkedList<ListItem<T>>(items);
                return items.Count;
            }
            else
            {
                linkedList = new LinkedList<ListItem<T>>();
                return 0;
            }
        }

        private void PushList()
        {
            SetProperty(itemsSourceName, linkedList.ToArray());
        }

        protected override void DoAction(string name, PageModelActionEventArgs actionArgs)
        {
			if (name == ModelNames.UnmappedAction && actionArgs.State == Interfaces.Input.ButtonStates.Press)
            {
                switch (actionArgs.ActionName)
                {
                    case ModelNames.ButtonNext:
                        FocusNext();
                        break;
                    case ModelNames.ButtonPrev:
                        FocusPrev();
                        break;
                    case ModelNames.ButtonSelect:
                        SelectItem();
                        break;
                }
            }
            else
            {
                base.DoAction(name, actionArgs);
            }
        }

        protected void SelectItem()
        {
            var focusedNode = GetFocusedNode();

            if (focusedNode != null)
                focusedNode.Value.Click();
        }

        private LinkedListNode<ListItem<T>> GetFocusedNode()
        {
            var focusedItem = linkedList.FirstOrDefault(i => i.Focused);
            if (focusedItem != null)
                return linkedList.Find(focusedItem);
            else
                return null;
        }

        private bool TryOtherBatch(int skipDelta)
        {
            if (skip + skipDelta < 0)
                return false;

            var oldList = linkedList;
            skip += skipDelta;
            QueryItems();
            if (linkedList.Count > 0)
            {
                PushList();
                return true;
            }
            else
            {
                skip -= skipDelta;
                linkedList = oldList;
                return false;
            }
        }

        protected void FocusNext()
        {
            var focused = GetFocusedNode();

            if (focused != null)
            {
                if (focused.Next != null)
                {
                    focused.Value.Focused = false;
                    focused.Next.Value.Focused = true;
                }
                else
                {
                    if (TryOtherBatch(itemsPerPage))
                    {
                        linkedList.First.Value.Focused = true;
                    }
                }
            }
            else
            {
                if (linkedList.Any())
                {
                    linkedList.First.Value.Focused = true;
                }
            }
        }

        protected void FocusPrev()
        {
            var focused = GetFocusedNode();

            if (focused != null)
            {
                if (focused.Previous != null)
                {
                    focused.Value.Focused = false;
                    focused.Previous.Value.Focused = true;
                }
                else
                {
                    if (TryOtherBatch(-itemsPerPage))
                    {
                        linkedList.Last.Value.Focused = true;
                    }
                }
            }
            else
            {
                if (linkedList.Any())
                {
                    linkedList.Last.Value.Focused = true;
                }
            }
        }
    }
}
