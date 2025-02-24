﻿//Main page's Xaml's CS code for Snake Game. Implemneted by Chase CANNNING and Jack MCINTYRE for CS3500, Fall of 2023

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
    
   
    GameController.GameController gc;
    
    public MainPage()
    {
        InitializeComponent();
        
        //creates game controller for the current player
        gc = new GameController.GameController(nameText.Text);

        //set the gc of the wolrd pannel to the same gc
        worldPanel.setGameController(gc);
        
        
        gc.WorldBuilt += enableCommandEntry;
        gc.UpdateArrived += OnFrame;
        gc.ErrorArrived += NetworkErrorHandler;
    }

    void OnTapped(object sender, EventArgs args)
    {
        keyboardHack.Focus();
    }


    /// <summary>
    /// Once a command has been typed into the KeyBoardHack box parse the information
    /// and if it is a valid command send a moving object to the server. Valid commands include 
    /// W,A,S,D. any other command will be ignored. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
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

    /// <summary>
    /// Used to display a Alert when a error has arisen in the connection between server and client. 
    /// </summary>
    private void NetworkErrorHandler()
    {
        Dispatcher.Dispatch(() => DisplayAlert("Error", "Failure to connect or maintain connection with server, try and re-connect", "OK"));

        //allow user to attempt to reconnect
        enableConnect();
    }



    /// <summary>
    /// Event handler for the connect button
    /// The name entered in the name box will be used to indetify the snake
    /// If the name is empty, server name is empty, or the name is longer than 16 charaters a 
    /// alert will be displayed. 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void ConnectClick(object sender, EventArgs args)
    {
        
        gc.playerName=nameText.Text;


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

        enableConnect();
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
      "Implementation by Norman CANNING and Jack MCINTYRE\n" +
        "CS 3500 Fall 2023, University of Utah", "OK");
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
            keyboardHack.IsEnabled = true;
        });
        
    }
    /// <summary>
    /// Flips the connect buttons isEnabled bool
    /// </summary>
    private void enableConnect()
    {
        Dispatcher.Dispatch(() =>
        {
            connectButton.IsEnabled = !connectButton.IsEnabled;
        });

    }

  


}