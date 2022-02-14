namespace WebRequestExample;

public class App {
    private int primaryMenuIndex = 0;

    private string error = "";

    private RequestManager reqObj;

    public App(){
        this.reqObj = new RequestManager();
    }

    public void init(){
        Console.Clear();
        this.showMenu();
    }

    private void showMenu(){
        ConsoleKeyInfo pressedKey = new ConsoleKeyInfo();

        while(true){
            if(!string.IsNullOrEmpty(this.error)) Console.WriteLine(this.error);
            Console.WriteLine(AppData.MenuContent[this.primaryMenuIndex]);

            while(Console.KeyAvailable == false) Thread.Sleep(250);
            //string input = Console.ReadLine();
            pressedKey = Console.ReadKey(true);
            int pressedKeyInteger;
            String keyChar = pressedKey.KeyChar.ToString();
            bool isNumber = int.TryParse(keyChar, out pressedKeyInteger);

            if(pressedKey.Key.ToString() == "Escape"){
                if(this.primaryMenuIndex==0){
                    Console.WriteLine("Press any key to exit...");
                    while(Console.KeyAvailable == false) Thread.Sleep(250);
                    pressedKey = Console.ReadKey();
                    string pressedKeyString = pressedKey.Key.ToString();
                    if (!string.IsNullOrEmpty(pressedKeyString)) break;
                }
                else{
                    Console.WriteLine("Press Escape again to quit, OR, Backspace to go back to previous menu.");
                    while(Console.KeyAvailable == false) Thread.Sleep(250);
                    pressedKey = Console.ReadKey(true);
                    if(pressedKey.Key.ToString() == "Backspace"){
                        Console.Clear();
                        this.error = "";
                        this.primaryMenuIndex = 0;   
                    }
                    else if(pressedKey.Key.ToString() == "Escape"){
                        Console.Clear();
                        break;
                    }
                }
            }
            else if(isNumber){
                int selectedMenu = Int32.Parse(keyChar);
                //Console.WriteLine(keyChar, selectedMenu);
                if(selectedMenu == 0){
                    Console.Clear();
                    this.error = "Error: Option unavailable.";
                }
                else if(this.primaryMenuIndex==0){
                    Console.Clear();
                    if(selectedMenu > 2) this.error = "Error: Option unavailable.";
                    else{
                        this.error = "";
                        this.primaryMenuIndex = selectedMenu;
                    }
                }
                else if(this.primaryMenuIndex==1){
                    Console.Clear();
                    if(selectedMenu > 9) this.error = "Error: Option unavailable.";
                    else{
                        this.error = "";
                        
                        reqObj.MakeRequest(selectedMenu);

                        Console.WriteLine("Press any key to continue...");
                        while(Console.KeyAvailable == false) Thread.Sleep(250);
                        Console.ReadKey();
                        Console.Clear();
                    }
                }
                else if(this.primaryMenuIndex==2){
                    Console.Clear();
                    this.error = "Error: Option unavailable.";
                }
            }
            else{
                Console.Clear();
                this.error = "Error: Invalid option.";
            }
        }
    }
}

public static class AppData {

    public static readonly string[] MenuContent = new string[] {
@"
Please press option number as shown below.
        
    [1]:    Make a Request
    [2]:    Display Detailed Info
    ------------------------------------------------------------------------------------
    [Esc]:  Quit
",

@"
Please press option number as shown below.
        
    [1]:    Login
    [2]:    Validate Token
    [3]:    Logout
    ------------------------------------------------------------------------------------
    [4]:    Send Password Reset Email
    [5]:    Reset Password
    ------------------------------------------------------------------------------------
    [6]:    Get My User Data
    ------------------------------------------------------------------------------------
    [7]:    Get My Progress Data
    [8]:    Create My Progress Data
    [9]:    Update My Progress Data
    ------------------------------------------------------------------------------------
    [Esc]:  Back/Quit
",

@"
Display full API info here...

    ------------------------------------------------------------------------------------
    [Esc]:  Back/Quit
"
    };

}