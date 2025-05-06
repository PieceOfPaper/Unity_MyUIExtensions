using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PowerfulMVP.Example
{
    public class MainMenu : PresenterTemplate<MainMenu, MainMenu.MyContext>
    {
        public override Setting setting => new Setting()
        {
            depthGroupID = 1,
        };

        public class MyContext : Context
        {
        
        }

        protected override void OnOpen()
        {
            base.OnOpen();
        }

        public void OnClick_Notice()
        {
            m_Manager.Open<NoticePopup>();
        }
    }
}
