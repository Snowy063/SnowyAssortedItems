using System;
using Microsoft.Xna.Framework;

namespace Celeste.Mod.SnowyAssortedItems {
    public class SnowyAssortedItemsModule : EverestModule {
        public static SnowyAssortedItemsModule Instance { get; private set; }

        public override Type SettingsType => typeof(SnowyAssortedItemsModuleSettings);
        public static SnowyAssortedItemsModuleSettings Settings => (SnowyAssortedItemsModuleSettings) Instance._Settings;

        public override Type SessionType => typeof(SnowyAssortedItemsModuleSession);
        public static SnowyAssortedItemsModuleSession Session => (SnowyAssortedItemsModuleSession) Instance._Session;

        public SnowyAssortedItemsModule() {
            Instance = this;
#if DEBUG
            // debug builds use verbose logging
            Logger.SetLogLevel(nameof(SnowyAssortedItemsModule), LogLevel.Verbose);
#else
            // release builds use info logging to reduce spam in log files
            Logger.SetLogLevel(nameof(SnowyAssortedItemsModule), LogLevel.Info);
#endif
        }

        public override void Load() {
            // TODO: apply any hooks that should always be active
        }

        public override void Unload() {
            // TODO: unapply any hooks applied in Load()
        }
    }
}