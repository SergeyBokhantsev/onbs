﻿using Interfaces;
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

        public abstract string Caption { get; }
        public abstract void Select();
        public abstract void Increase();
        public abstract void Decrease();

        protected ConfigItem(IConfig config, Dictionary<string, string> props, string captionPrefix)
        {
            this.config = config;

            if (props.ContainsKey("Type"))
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

                    case "Boolean":
                        return new BooleanConfigItem(config, properties, captionPrefix);

                    case "CycleString":
                        return new CycleStringConfigItem(config, properties, captionPrefix);

                    case "IntRange":
                        return new IntegerRangeConfigItem(config, properties, captionPrefix);

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

    public class CycleStringConfigItem : ConfigItem
    {
        private readonly string[] allValues;
        private int index;

        public override string Caption
        {
            get
            {
                return string.Concat(captionPrefix, config.GetString(settingName));
            }
        }

        public CycleStringConfigItem(IConfig config, Dictionary<string, string> properties, string captionPrefix)
            : base(config, properties, captionPrefix)
        {
            allValues = properties["Values"].Split(',');
            index = Array.IndexOf(allValues, config.GetString(settingName));

            if (index < 0)
            {
                index = 0;
                config.Set(settingName, allValues[index]);
            }
        }

        public override void Select()
        {
            Increase();
        }

        public override void Increase()
        {
            index++;
            if (index == allValues.Length)
                index = 0;

            config.Set(settingName, allValues[index]);
        }

        public override void Decrease()
        {
            index--;
            if (index < 0)
                index = allValues.Length - 1;

            config.Set(settingName, allValues[index]);
        }
    }

    public class BooleanConfigItem : ConfigItem
    {
        public override string Caption
        {
            get
            {
                return string.Concat(captionPrefix, config.GetBool(settingName) ? "Yes" : "No");
            }
        }

        public BooleanConfigItem(IConfig config, Dictionary<string, string> properties, string captionPrefix)
            :base(config, properties, captionPrefix)
        {
        }

        public override void Select()
        {
            config.InvertBoolSetting(settingName);
        }

        public override void Increase()
        {
            config.Set(settingName, true);
        }

        public override void Decrease()
        {
            config.Set(settingName, false);
        }
    }

    public class CycleEnumConfigItem : ConfigItem
    {
        private readonly string[] allValues;

        private int index;

        public override string Caption
        {
            get
            {
                return string.Concat(captionPrefix, allValues[index]);
            }
        }

        public CycleEnumConfigItem(IConfig config, Dictionary<string, string> properties, string captionPrefix)
            :base(config, properties, captionPrefix)
        {
            var values = Enum.GetValues(type);
            allValues = new string[values.Length];

            for(int i=0; i<values.Length; ++i)
            {
                allValues[i] = values.GetValue(i).ToString();
            }

            index = Array.IndexOf(allValues, config.GetString(settingName));

            if (index == -1)
            {
                index = 0;
                config.Set(ConfigNames.LogLevel, allValues[index]);
            }
        }

        public override void Select()
        {
            Increase();
        }

        public override void Increase()
        {
            index++;
            if (index == allValues.Length)
                index = 0;

            config.Set(ConfigNames.LogLevel, allValues[index]);
        }

        public override void Decrease()
        {
            index--;
            if (index < 0)
                index = allValues.Length - 1;

            config.Set(ConfigNames.LogLevel, allValues[index]);
        }
    }

    public class IntegerRangeConfigItem : ConfigItem
    {
        private readonly int step;
        private readonly int min;
        private readonly int max;

        public override string Caption
        {
            get { return string.Concat(captionPrefix, " ", config.GetInt(settingName)); }
        }

        public IntegerRangeConfigItem(IConfig config, Dictionary<string, string> properties, string captionPrefix)
            :base(config, properties, captionPrefix)
        {
            this.step = int.Parse(properties["Step"]);
            this.min = int.Parse(properties["Min"]);
            this.max = int.Parse(properties["Max"]);
        }

        public override void Select()
        {
            var value = config.GetInt(settingName);

            if (value == max)
            {
                value = min;
                config.Set(settingName, value);
            }
            else
                Increase();
        }

        public override void Increase()
        {
            var value = config.GetInt(settingName);
            value = Math.Min(max, value + step);
            config.Set(settingName, value);
        }

        public override void Decrease()
        {
            var value = config.GetInt(settingName);
            value = Math.Max(min, value - step);
            config.Set(settingName, value);
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
                    else
                    {
                        var mappedPageAction = mappedAction as MappedPageAction;

                        if (mappedPageAction != null)
                        {
                            ListItem<ConfigItem>.PrepareItem(hc.SyncContext, ref rotaryItems, null, (s, e) => Action(new PageModelActionEventArgs(mappedAction.ButtonActionName, Interfaces.Input.ButtonStates.Press)), mappedAction.Caption);
                        }
                        else
                            throw new Exception("Unknown action");
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

        protected override async Task DoAction(string name, PageModelActionEventArgs actionArgs)
        {
            if (name.StartsWith("SaveAndReturn"))
            {
                hc.Config.Save();
                var pageDescriptorName = name.Split(':')[1];
                hc.GetController<IUIController>().ShowPage(pageDescriptorName, null, null);
            }
            else if (name == ModelNames.UnmappedAction)
            {
                if (actionArgs.ActionName == ModelNames.ButtonF1)
                {
                    var index = SelectedIndexAbsolute;
                    if (index >= 0 && index < rotaryItems.Count)
                    {
                        rotaryItems[index].Value.Increase();
                        rotaryItems[index].Caption = rotaryItems[index].Value.Caption;
                    }
                }
                else if (actionArgs.ActionName == ModelNames.ButtonF2)
                {
                    var index = SelectedIndexAbsolute;
                    if (index >= 0 && index < rotaryItems.Count)
                    {
                        rotaryItems[index].Value.Decrease();
                        rotaryItems[index].Caption = rotaryItems[index].Value.Caption;
                    }
                }
                else
                {
                    await base.DoAction(name, actionArgs);
                } 
            }
            else
            {
                await base.DoAction(name, actionArgs);
            }           
        }
    }
}
