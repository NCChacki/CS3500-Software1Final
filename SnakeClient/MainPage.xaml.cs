using Microsoft.Maui.Layouts;
using NetworkUtil;
using Windows.Gaming.Input;


namespace SnakeGame;

public partial class MainPage : ContentPage
{
    //for use in the SendPlayerName method.
    private string playerName;

    //The delegate the holds the SendPlayerNameMethod.
    //private delegate void NameSender(SocketState socketState);

    private Action<SocketState> nameSender;
    
    //TODO check if this is allowed
    GameController.GameController gc;
    
    public MainPage()
    {
        InitializeComponent();
        
        
        gc = new GameController.GameController(nameText.Text);

        worldPanel.setGameController(gc);
        

        gc.WorldBuilt += enableCommandEntry;
        gc.UpdateArrived += OnFrame;
        gc.ErrorArrived += NetworkErrorHandler;
       



    }

    void OnTapped(object sender, EventArgs args)
    {
        keyboardHack.Focus();
    }

    void OnTextChanged(object sender, TextChangedEventArgs args)
    {
        Entry entry = (Entry)sender;
        String text = entry.Text.ToLower();
        if (text == "w")
        {
            gc.textChanged("up");
        }
        else if (text == "a")
        {
            gc.textChanged("left");
        }
        else if (text == "s")
        {
            gc.textChanged("down");
        }
        else if (text == "d")
        {
            gc.textChanged("right");
        }
        entry.Text = "";

        
    }

    private void NetworkErrorHandler()
    {
        Dispatcher.Dispatch(() => DisplayAlert("Error", "Disconnected from server", "OK"));

        enableCommandEntry();
    }


    /// <summary>
    /// Event handler for the connect button
    /// We will put the connection attempt interface here in the view.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void ConnectClick(object sender, EventArgs args)
    {
        playerName= nameText.Text;


        
        if (serverText.Text == "")
        {
            DisplayAlert("Error", "Please enter a server address", "OK");
            return;
        }
        if (nameText.Text == "")
        {
            DisplayAlert("Error", "Please enter a name", "OK");
            return;
        }
        if (nameText.Text.Length > 16)
        {
            DisplayAlert("Error", "Name must be less than 16 characters", "OK");
            return;
        }

        
            gc.Connect(serverText.Text);
        enableCommandEntry();
        

        keyboardHack.Focus();
    }

    /// <summary>
    /// Use this method as an event handler for when the controller has updated the world
    /// </summary>
    public void OnFrame()
    {
        Dispatcher.Dispatch(() => graphicsView.Invalidate());
    }

    private void ControlsButton_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("Controls",
                     "W:\t\t Move up\n" +
                     "A:\t\t Move left\n" +
                     "S:\t\t Move down\n" +
                     "D:\t\t Move right\n",
                     "OK");
    }

    private void AboutButton_Clicked(object sender, EventArgs e)
    {
        DisplayAlert("About",
      "SnakeGame solution\nArtwork by Jolie Uk and Alex Smith\nGame design by Daniel Kopta and Travis Martin\n" +
      "Implementation by ...\n" +
        "CS 3500 Fall 2022, University of Utah", "OK");
    }

    private void ContentPage_Focused(object sender, FocusEventArgs e)
    {
        if (!connectButton.IsEnabled)
            keyboardHack.Focus();
    }

    /// <summary>
    /// Responds to a world being built allowing a user to change the keyBoardHack enrty 
    /// and make commands
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void enableCommandEntry()
    {
        Dispatcher.Dispatch(() =>
        {
            keyboardHack.IsEnabled = !keyboardHack.IsEnabled;
        });
        
    }

  


}