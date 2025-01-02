using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PowerfulUI
{
    public interface ISelectableTransitionApplier
    {
        public void DoStateTransition(int state, bool instant);
    }

}