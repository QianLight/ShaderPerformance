using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GSDK.RNU
{
    public class ReactPickerManager : BaseViewManager
    {
        public static string PickerName = "RNUPicker";
        override public string GetName()
        {
            return PickerName;
        }

        protected override BaseView createViewInstance()
        {
            return new ReactPicker(GetName());
        }

        [ReactProp(name = "items")]
        public void SetItems(ReactPicker view, ArrayList items)
        {
            view.SetItems(items);
        }

        [ReactProp(name = "selected")]
        public void SetValue(ReactPicker view, int selected)
        {
            view.SetValue(selected);
        }

        [ReactProp(name = "enabled")]
        public void SetEnabled(ReactPicker view, bool enabled)
        {
            view.SetEnabled(enabled);
        }

        [ReactProp(name = "mode")]
        public void SetMode(ReactPicker view, string mode)
        {
            view.SetMode(mode);
        }

        [ReactProp(name = "arrowImageSource")]
        public void SetArrowImage(ReactPicker view, string uri)
        {
            view.SetArrowImage(uri);
        }

        [ReactProp(name = "captionImageSource")]
        public void SetCaptionImage(ReactPicker view, string uri)
        {
            view.SetCaptionImage(uri);
        }

        [ReactProp(name = "itemColor")]
        public void SetItemTextColor(ReactPicker view, long color)
        {
            view.SetItemTextColor(color);
        }

        [ReactProp(name = "disabled", defaultBoolean = false)]
        public void SetDisable(BaseView view, bool disable)
        {
            ReactPicker picker = (ReactPicker) view;
            picker.SetDisable(disable);
        }

        [ReactProp(name = "arrowSize")]
        public void SetArrowSize(ReactPicker view, int size)
        {
            view.SetArrowSize(size);
        }

        [ReactProp(name = "checkedSize")]
        public void SetCheckedSize(ReactPicker view, int size)
        {
            view.SetCheckedSize(size);
        }

        [ReactProp(name = "captionTextSize")]
        public void SetCaptionTextSize(ReactPicker view, int size)
        {
            view.SetCaptionTextSize(size);
        }

        [ReactProp(name = "itemTextSize")]
        public void SetItemTextSize(ReactPicker view, int size)
        {
            view.SetItemTextSize(size);
        }

        [ReactProp(name = "listHeight")]
        public void SetListHeight(ReactPicker view, int size)
        {
            view.SetListHeight(size);
        }

        [ReactProp(name = "itemHeight")]
        public void SetItemHeight(ReactPicker view, int size)
        {
            view.SetItemHeight(size);
        }
        public override Hashtable GetExportedCustomDirectEventTypeConstants()
        {
            return new Hashtable{
                {"onValueChange", new Hashtable{
                    {sRegistration, "onValueChange"}
                }},
            };
        }
    }
}