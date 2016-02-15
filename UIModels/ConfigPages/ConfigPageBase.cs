using Interfaces;
using Interfaces.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UIModels.MultipurposeModels;

namespace UIModels
{
    public abstract class ConfigItem
    {
        protected readonly IConfig config;
        protected readonly string settingName;
        protected readonly string captionPrefix;
        protected readonly Type type;

        public string Caption { get; protected set; }
        public abstract void Select();

        protected ConfigItem(IConfig config, Dictionary<string, string> props, string captionPrefix)
        {
            this.config = config;
            type = Type.GetType(props["Type"]);
            settingName = props["SettingName"];
            this.captionPrefix = captionPrefix;
        }

        public static ConfigItem Create(IConfig config, string action, string captionPrefix)
        {
            var propertyPairs = action.Split(';');

            var properties = (from pair in propertyPairs
                              let items = pair.Split(':')
                              select new KeyValuePair<string, string>(items[0], items[1])).ToDictionary(x => x.Key, x => x.Value);

            if (!properties.ContainsKey("Behavior"))
                throw new NotImplementedException(action);

            try
            {
                switch (properties["Behavior"])
                {
                    case "CycleEnum":
                        return new CycleEnumConfigItem(config, properties, captionPrefix);

                    default:
                        throw new NotImplementedException(action);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Unable to process Config action: '{0}'. Message was: {1}", action, ex.Message), ex);
            }
        }
    }

    public class CycleEnumConfigItem : ConfigItem
    {
        public CycleEnumConfigItem(IConfig config, Dictionary<string, string> properties, string captionPrefix)
            :base(config, properties, captionPrefix)
        {
            var v = config.GetString(settingName);
            Caption = string.Concat(captionPrefix, v);
        }

        public override void Select()
        {
            Caption = Guid.NewGuid().ToString();
        }
    }

    public class ConfigPageBase : RotaryListModel<ConfigItem>
    {
        private readonly List<ListItem<ConfigItem>> rotaryItems = new List<ListItem<ConfigItem>>(10);

        public ConfigPageBase(string viewName, IHostController hc, MappedPage pageDescriptor)
            :base(viewName, hc, pageDescriptor, "list", 10)
        {
            foreach (var mappedAction in pageDescriptor.ButtonsMap)
            {
                if (mappedAction.ButtonActionName.StartsWith("ConfigItem_"))
                {
                    var customAction = mappedAction as MappedCustomAction;

                    if (customAction != null)
                    {
                        var configItem = ConfigItem.Create(hc.Config, customAction.CustomActionName, customAction.Caption);
                        ListItem<ConfigItem>.PrepareItem(hc.SyncContext, ref rotaryItems, configItem, OnItemClick, configItem.Caption);
                    }
                }
            }
        }

        private void OnItemClick(object sender, EventArgs e)
        {
            var listItem = sender as ListItem<ConfigItem>;
            listItem.Value.Select();
            listItem.Caption = listItem.Value.Caption;
        }

        protected override IList<ListItem<ConfigItem>> QueryItems(int skip, int take)
        {
            return rotaryItems.Skip(skip).Take(take).ToList();
        }

        protected override void DoAction(string name, PageModelActionEventArgs actionArgs)
        {
            if (name.StartsWith("SaveAndReturn"))
            {
                hc.Config.Save();
                var pageDescriptorName = name.Split(':')[1];
                hc.GetController<IUIController>().ShowPage(pageDescriptorName, null, null);
            }
            else
            {
                base.DoAction(name, actionArgs);
            }           
        }
    }
}
