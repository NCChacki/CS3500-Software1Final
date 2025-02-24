﻿//World Panel Class for Snake game. Implemneted by Chase CANNNING and Jack MCINTYRE for CS3500, Fall of 2023

using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using IImage = Microsoft.Maui.Graphics.IImage;
#if MACCATALYST
using Microsoft.Maui.Graphics.Platform;
#else
using Microsoft.Maui.Graphics.Win2D;
#endif
using Color = Microsoft.Maui.Graphics.Color;
using System.Reflection;
using Microsoft.Maui;
using System.Net;
using Font = Microsoft.Maui.Graphics.Font;
using SizeF = Microsoft.Maui.Graphics.SizeF;
using Model;
using Windows.UI.Input.Inking;
using System;
using System;

using System.Drawing.Printing;
using Microsoft.Maui.Controls.Shapes;
using System.Security.Cryptography;
using Microsoft.UI;
using Colors = Microsoft.Maui.Graphics.Colors;
using System.Runtime.InteropServices;

namespace SnakeGame;
public class WorldPanel : StackLayout, IDrawable
{
    /// <summary>
    /// Sprite for games walls
    /// </summary>
    private IImage wall;

    /// <summary>
    /// Sprite for games background
    /// </summary>
    private IImage background;

    /// <summary>
    /// Sprite for top scoring snakes crown
    /// </summary>
    private IImage crown;

    /// <summary>
    /// graphics view for the world pannel
    /// </summary>
    private GraphicsView graphicsView;


    private bool initializedForDrawing = false;

    // A delegate for DrawObjectWithTransform
    // Methods matching this delegate can draw whatever they want onto the canvas
    public delegate void ObjectDrawer(object o, ICanvas canvas);

    /// <summary>
    /// Game Controller referenced by the view
    /// </summary>
    private GameController.GameController gc;


    /// <summary>
    /// List of the snake colors to be used
    /// </summary>
    private List<Color> snakeColors = new List<Color> { Colors.HotPink, Colors.Red, Colors.Orange, Colors.Yellow, Colors.LimeGreen, Colors.Blue, Colors.Turquoise, Colors.Black };


    private IImage loadImage(string name)
    {
        Assembly assembly = GetType().GetTypeInfo().Assembly;
        string path = "SnakeClient.Resources.Images";
        using (Stream stream = assembly.GetManifestResourceStream($"{path}.{name}"))
        {
#if MACCATALYST
            return PlatformImage.FromStream(stream);
#else
            return new W2DImageLoadingService().FromStream(stream);
#endif
        }
    }


    /// <summary>
    /// Default constructor for the WorldPannel
    /// </summary>
    public WorldPanel() { }

    public void Invalidate()
    {
        graphicsView.Invalidate();
    }

    /// <summary>
    /// Sets the sprites for the wall, crown and background and sets intisalizedForDrawing to true. 
    /// </summary>
    private void InitializeDrawing()
    {

        crown = loadImage("goldcrownrealistic.png");
        wall = loadImage("wallsprite.png");
        background = loadImage("background.png");

        initializedForDrawing = true;
    }

    private bool CheckTheBorder(Vector2D point)
    {
        return (point.X == 1000 || point.Y == -1000) || (point.Y == 1000 || point.Y == -100); 
    }


    /// <summary>
    /// World Pannels draw method, ititializes the drawing then draws on all snakes, walls and powerUps
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="dirtyRect"></param>
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (!initializedForDrawing)
            InitializeDrawing();

        // undo previous transformations from last frame
        canvas.ResetState();


        //moves cnavs to currrent location of the player

        canvas.Translate((float)-gc.playerX + 450, (float)-gc.playerY + 450);

        canvas.DrawImage(background, (float)-gc.worldSize / 2, (float)-gc.worldSize / 2, (float)gc.worldSize, (float)gc.worldSize);

        string PlayerName = gc.playerName;

        HorizontalAlignment NameTag = HorizontalAlignment.Center;

        //Sets size of the snake and makes them rounded. 
        canvas.StrokeSize = 10;
        canvas.StrokeLineCap = LineCap.Round;

        //makes sure the wolrd has been created before calling from it. 
        if (gc.world != null)
        {
            //lock the world to elimate possible race conditions with its data
            lock (gc.world)
            {

                foreach (Power power in gc.world.Powerups.Values)
                    DrawObjectWithTransform(canvas, power,
                      power.loc.X, power.loc.Y, 0,
                      drawPowerUp);

                //draw each of the wall segments on the canvas.
                foreach (Wall wall in gc.world.Walls.Values)
                {
                    foreach (Vector2D segment in wallSegments(wall))
                        DrawObjectWithTransform(canvas, segment, segment.X - 50, segment.Y - 50, 0, drawWall);
                }

                //chack to make sure player data has been processed
                if (gc.world.Players != null)
                {
                    string nameOfTopScore = "";
                    foreach (Snake snake in gc.world.Players.Values)
                    {
                        if (nameOfTopScore == "")
                            nameOfTopScore = snake.name;
                        if (gc.world.Players[nameOfTopScore].score < snake.score)
                            nameOfTopScore = snake.name;

                    }


                    foreach (Snake snake in gc.world.Players.Values)
                    {

                        Vector2D lastSegment = snake.body.FirstOrDefault();
                        //changes the color of snake depending on its ID, first 8 should be unque after that no gaurentee
                        canvas.StrokeColor = snakeColors[snake.snake % 8];

                        //loop throgh and draw all segments of each snake. 
                        foreach (Vector2D currentSegment in snake.body)
                        {
                            if(CheckTheBorder(lastSegment) && CheckTheBorder(currentSegment))
                            {
                                lastSegment = currentSegment;
                                continue;
                            }

                            double segmentLength;
                            Vector2D angle;

                            //calculates the length and angle of segment to be drawn.
                            segmentLength = (currentSegment - lastSegment).Length();
                            angle = (lastSegment - currentSegment);
                            angle.Normalize();


                            DrawObjectWithTransform(canvas, segmentLength, currentSegment.X, currentSegment.Y, angle.ToAngle(), drawSnakeSegment);
                            lastSegment = currentSegment;

                            //set color and font size of snake names.
                            canvas.FontColor = Colors.White;
                            canvas.FontSize = 18;


                        }

                        //get the biggest snake
                        Snake biggest = gc.world.Players[nameOfTopScore];

                        //Draw the Crown
                        canvas.DrawImage(crown, (float)biggest.body.Last<Vector2D>().X - 10, (float)biggest.body.Last<Vector2D>().Y - 22, 20, 20);

                        //Draw the name of the player
                        canvas.DrawString(snake.name + ": " + snake.score, (float)snake.body.Last<Vector2D>().X, (float)snake.body.Last<Vector2D>().Y - 25, NameTag);

                    }

                }

            }
        }



    }


    /// <summary>
    /// Used to drawellipses to repersent the power ups on the given canvas. 
    /// </summary>
    /// <param name="o"></param>
    /// <param name="canvas"></param>
    public void drawPowerUp(object o, ICanvas canvas)
    {
        Power power = o as Power;
        int width = 16;

        //use power ID to determine its color. 
        if (power.power % 2 == 0)
        {
            canvas.FillColor = Colors.OrangeRed;
        }
        else
            canvas.FillColor = Colors.IndianRed;

        canvas.FillEllipse(-(width / 2), -(width / 2), width, width);
    }

    /// <summary>
    /// Used to draw the walls of the game on the given canvas
    /// </summary>
    /// <param name="o"></param>
    /// <param name="canvas"></param>
    public void drawWall(object o, ICanvas canvas)
    {

        int width = 50;
        canvas.DrawImage(wall, (float)width / 2, (float)width / 2, (float)width, (float)width);
    }


    /// <summary>
    /// Draws snake segmnets of a given length. 
    /// </summary>
    /// <param name="o"></param>
    /// <param name="canvas"></param>
    public void drawSnakeSegment(object o, ICanvas canvas)
    {
        Double length = (Double)o;

        canvas.DrawLine(0, 0, 0, (float)-length);


    }

    /// <summary>
    /// Setter for game Controller
    /// </summary>
    /// <param name="controller"></param>
    public void setGameController(GameController.GameController controller)
    {
        this.gc = controller;
    }




    /// <summary>
    /// This method performs a translation and rotation to draw an object.
    /// </summary>
    /// <param name="canvas">The canvas object for drawing onto</param>
    /// <param name="o">The object to draw</param>
    /// <param name="worldX">The X component of the object's position in world space</param>
    /// <param name="worldY">The Y component of the object's position in world space</param>
    /// <param name="angle">The orientation of the object, measured in degrees clockwise from "up"</param>
    /// <param name="drawer">The drawer delegate. After the transformation is applied, the delegate is invoked to draw whatever it wants</param>
    private void DrawObjectWithTransform(ICanvas canvas, object o, double worldX, double worldY, double angle, ObjectDrawer drawer)
    {
        // "push" the current transform
        canvas.SaveState();

        canvas.Translate((float)worldX, (float)worldY);
        //change the rotation of a drawing(patterns for a snake)
        canvas.Rotate((float)angle);


        //Draws the snake or object.
        drawer(o, canvas);

        // "pop" the transform
        //puts the changes in the view?
        canvas.RestoreState();
    }

    /// <summary>
    /// Method used to calulate how many wall segemnts lie between two points in a 
    /// Wall Vector2D, and calulates the cordinates of those wall segements
    /// </summary>
    /// <param name="wall"></param>
    /// <returns></returns>
    private List<Vector2D> wallSegments(Wall wall)
    {
        //return list
        List<Vector2D> returnList = new List<Vector2D>();

        //begin and end for the creation of segments
        double begin;
        double end;


        //drawing on the y axis if the x-cordintaes are the same. 
        if (wall.p1.X == wall.p2.X)
        {
            //check for the smaller of the two Ys
            if (wall.p1.Y < wall.p2.Y)
            {
                begin = wall.p1.Y;
                end = wall.p2.Y;
            }
            else
            {
                begin = wall.p2.Y;
                end = wall.p1.Y;
            }

            //creat the points between the begin and end, incremnets of 50, will not include last point
            for (double i = begin; i < end; i += 50)
                returnList.Add(new Vector2D(wall.p1.X, i));
            returnList.Add(new Vector2D(wall.p2.X, end));

        }
        else
        {
            //check for the smaller of the two Xs
            if (wall.p1.X < wall.p2.X)
            {
                begin = wall.p1.X;
                end = wall.p2.X;
            }
            else
            {
                begin = wall.p2.X;
                end = wall.p1.X;
            }

            //creat the points between the begin and end, incremnets of 50, will not include last point
            for (double i = begin; i < end; i += 50)
                returnList.Add(new Vector2D(i, wall.p1.Y));
            returnList.Add(new Vector2D(end, wall.p2.Y));

        }

        return returnList;
    }


}
