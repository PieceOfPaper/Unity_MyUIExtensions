using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PowerfulMVP.Example
{
    public class NoticePopup : PresenterTemplate<NoticePopup, NoticePopup.MyContext>
    {
        public override Setting setting => new Setting()
        {
            depthGroupID = 3,
        };

        public class MyContext : Context
        {
        
        }

        protected override void OnOpen()
        {
            base.OnOpen();
        }
    }
}
