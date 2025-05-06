using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PowerfulMVP.Example
{
    public class TestPopup : PresenterTemplate<TestPopup, TestPopup.MyContext>
    {
        public override Setting setting => new Setting()
        {
            depthID = 3,
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
