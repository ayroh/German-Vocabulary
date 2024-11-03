using System;
using System.Collections;
using System.Collections.Generic;
using Utilities.Enums;


namespace Utilities.Signals
{
    public static class Signals
    {

        public static Action<bool> OnSetNewAnswer = delegate { };

        public static Action OnResetAnswer = delegate { };
    }
}
