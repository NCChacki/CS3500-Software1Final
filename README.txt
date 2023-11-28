
Notes for General Use:

To play the snake game you have to enter a valid server name along with a valid player name. A player name is valid if the name is between 1-16
characters. The default name for a character will be "player" this can only be changed by entering a different name in the name entry box before connecting.
If the client fails to connect to the given server within 3 seconds, the attempt will be stopped and a alert will be given. If the clients sever
Disconnects then an alert will be given, and the user will be prompted to try and reconnect. 

Movement commands can be made in the far-right entry box. Valid entries consist of W (up), A(left), S(down), and D (right). All other commands will be ignore 
and will not affect the movement of the snake.

Difficult design decisions: Choosing how to design process data was one of the most time consuming parts. We wanted to design that the ProcessData method. 
in a way that wasn’t bulky and was easy to read, this involved initial designs that consisted of lots of bool flags and if statements. But once we 
understood the basic logic of what we wanted to do we could start cutting down the lines of code. The other big difficult design problem was drawing the 
snake segments. It took a lot of thinking of how we wanted to break down that problem, but we ended up on deciding to make our approach full of 
for each nest so that when drawWithTransofm was used there was no confusion on what was being used as the parameters. We will admit this took us way. 
too long. To polish up the game we made sure our snakes had a rounded appearance and added a feature so that the top scoring snake could be identified 
by a little crown above its head. Makes an easy target :)
