== Read Me ==


Oh, you are reading this even though this was completely optional.

Anyway, this readme is for modders who want to create your own config screen.


First you need an OptionInterface, like this:
public class MyModConfig : OptionInterface
{
	public MyModConfig () : base(MyModScript.mod) //MyModScript.mod is static
	{
		
	}
}


And you need to add a function something like this in your PartialityMod:
public static OptionInterface LoadOI ()
{ //This will be called by CompletelyOptional right after Intro Roll.
	return new MyModConfig();
}


Now you can desing your own config menu in Initialize()!

private OpLabel myLabel;
public override void Initialize()
{
	base.Initialize(); //This must come first
	Tabs = new Tabs[1]; //Initialize Tabs. Number of tabs can go up to 20.
	Tabs[1] = new Tabs(); //You can add the name for tabs if you want.
	
	myLabel = new OpLabel(new Vector2(200f, 200f), new Vector2(300f, 20f), "This is the Label");
	Tabs[1].AddItem(myLabel);
}

Now when you enter the config menu, the menu will display a line of text(This is the Label) in the canvas.




== OptionInterface.Config ==

bool Configuable()
	Check if your mod can be configured.
	in default this will check every items you added to your interface
	and return the value respectly. However you can override this to more simple manner,
	like 'public override bool Configuable(){ return true; }' or 'return false;'.


Dictionary<string, string> config;
	This is how you access your config data.
	Every config is stored in form of string.
	If you want to get Color tho, then you can use 'HexToColor(hex)' that comes with.


== OptionInterface.DataManagement ==

string data
	This is how you access your data.
	This will call 'DataOnChange()' everytime you change it.

void LoadData()
	Load saved data from file.
	Called data will be differ by selected Save Slot.
	Call this and access data.
	This will NOT call 'DataOnChange()'

bool dataTinkered { get; }
	After you called 'LoadData()', this will show
	whether your saved data file has been tinkered or not.

bool SaveData()
	Save data which OptionInterface is currently holding.
	returns true when OI succeed saving.

string defaultData{ get{} }
	override this to your own defaultData.
	If you don't need saving feature, then leave it be.


