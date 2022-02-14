using System.Linq;

namespace WebRequestExample;

public class LoginData {
        
    public InternalData data { get; set; }

    public class InternalData {
        public string access_token { get; set; }
        public int expires { get; set; }
        public string refresh_token { get; set; }
    }

}

public class UserData {
        
    public InternalData data { get; set; }

    public class InternalData {
        public string id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
    }

}

// for enums go to end of this file
public class ProgressData {

    public InternalData[] data { get; set; }
    public ProcessedProgressData processedProgressData;

    public class InternalData {
        public string id { get; set; }
        public string updated_on { get; set; }
        public string user_id { get; set; }
        public string progress_data { get; set; }
    }

}

public class ProcessedProgressData {
            
    public EasterEgg[] easterEgg { get; set; }
    public LearningModule[] learningModule { get; set; }
    public bool completed_tutorial { get; set; }
    public ModuleIntro[] moduleIntros { get; set; }

    public class EasterEgg {
        public string id { get; set; }          // generated uuidv4
        public int identifier { get; set; }     // ProgressDataIdentifier enum
        public bool completed { get; set; }
    }
    public class LearningModule {
        public string id { get; set; }          // generated uuidv4
        public int identifier { get; set; }     // ProgressDataIdentifier enum
        public TouchPoint[] touchPoints { get; set; }
        public int completed { get; set; }      // CompletionStatus enum
        public bool seenCompletion { get; set; }

        public class TouchPoint {
            public string id { get; set; }      // generated uuidv4
            public int identifier { get; set; } // ProgressDataIdentifier enum
            public bool completed { get; set; }
        }
    }
    public class ModuleIntro {
        public int module { get; set; }         // LearningModuleNames enum
        public bool completed { get; set; }
    }

}

public static class EnumSets {

    public enum CompletionStatus: ushort {
        NotStarted = 0,
        InProgress = 1,
        Completed = 2
    }

    public enum LearningModuleNames: ushort {
        SpheresHub = 0,
        WelcomeToDS = 1,
        HistoryOfDS = 2,
        MeetYourCustomers = 3,
        HowWeOperate = 4,
        WereInventors = 5,
        OurImpact = 6,
        YourImpact = 7
    }

    public enum ProgressDataIdentifier: ushort {
        // Modules
        ModuleCustomers = 0,
        ModuleOperate = 1,
        ModuleInventors = 2,
        ModuleImpact = 3,
        ModuleHistory = 4,

        // Meet Our Customers
        CustomersRing = 5,
        CustomersFireStick = 6,
        CustomersEcho8 = 7,
        CustomersEcho15 = 8,
        CustomersEchoStudio = 9,
        CustomersDeviceCarousel = 10,

        // Our Impact
        ImpactVisualImpairments = 11,
        ImpactStayConnected1 = 12,
        ImpactStayConnected2 = 13,
        ImpactStaySafe = 14,
        ImpactDigitalBooks = 15,
        ImpactNarrative360 = 16,

        // Were Inventors
        InventorsDevices = 17,
        InventorsNarrative360 = 18,

        // How We Operate
        OperateAlexa = 19,
        OperateAmazonLab = 20,
        OperateRing = 21,
        OperateProjectKuiper = 22,
        OperateDeviceDesignGroup = 21,
        OperateHardware = 22,
        OperateDeviceSoftware = 23,
        OperateOperationsAndSupply = 24,
        OperateSalesAndMarketing = 25,

        // History of D&S
        History1994 = 26,
        History2004 = 27,
        History2007 = 28,
        History2014 = 29,
        History2015 = 30,
        History2021 = 31,

        //Other
        FirePhone = 32,
        RoRo = 33,
        DevicesTimeline = 34,
        Alexa = 35,
        Lab126 = 36,
        ONSChain = 37
    };

    public static readonly int[] ModuleRoomIdentifierRelations = new int[1] { 0 };
    public static readonly int[] CustomerRoomTouchpointRelations = new int[5] { 5, 6, 7, 8, 9 };
}


// This data model is just for demonstration purposes, which is why it is incomplete. To see the full schema, look at CustomerModuleData.json file.
public class CustomerModuleData {
    public int moduleRoom = EnumSets.ModuleRoomIdentifierRelations[0];
    public string moduleTitle = "Meet Your Customers";
    public int progressState = (int)EnumSets.CompletionStatus.NotStarted;
    public CheckpointDataDictionary checkpointIDDataDictionary = new CheckpointDataDictionary();

    // This is also incomplete
    public class CheckpointDataDictionary {
        public int[] _keys = new int[5] {
            EnumSets.CustomerRoomTouchpointRelations[0],
            EnumSets.CustomerRoomTouchpointRelations[1],
            EnumSets.CustomerRoomTouchpointRelations[2],
            EnumSets.CustomerRoomTouchpointRelations[3],
            EnumSets.CustomerRoomTouchpointRelations[4]
        };
        public bool[] _values = new bool[5] {
            false,
            false,
            false,
            false,
            false
        };
    }

    public void ModifyCustomModuleData(){
        Random rand = new Random();

        for(int i=0; i<5; i++){
            checkpointIDDataDictionary._values[i] = (rand.Next(0, 2) == 0) ? true : false;
        }
        int trueCheckpoints = checkpointIDDataDictionary._values.Where(c=>c == true).ToArray().Length;
        if(trueCheckpoints==5) this.progressState = (int)EnumSets.CompletionStatus.Completed;
        else if(trueCheckpoints>0) this.progressState = (int)EnumSets.CompletionStatus.InProgress;
    }
}

