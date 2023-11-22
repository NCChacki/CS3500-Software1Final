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

namespace SnakeGame;
public class WorldPanel : StackLayout, IDrawable
{
    private IImage wall;
    private IImage background;
    private IImage powerUpSprite;
    private GraphicsView graphicsView;

    private bool initializedForDrawing = false;

    // A delegate for DrawObjectWithTransform
    // Methods matching this delegate can draw whatever they want onto the canvas
    public delegate void ObjectDrawer(object o, ICanvas canvas);

    private GameController.GameController gc;

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

    public WorldPanel()
    {

    }

    public void Invalidate()
    {
        graphicsView.Invalidate();
    }

    private void InitializeDrawing()
    {
        wall = loadImage("wallsprite.png");
        background = loadImage("background.png");

        initializedForDrawing = true;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (!initializedForDrawing)
            InitializeDrawing();

        // undo previous transformations from last frame
        canvas.ResetState();

        canvas.StrokeSize = 10;




        if (gc.world != null)
        {

            Snake userSnake =gc.world.Players[gc.world.playerID];
            
            canvas.Translate((float)userSnake.body.Last<Vector2D>().X + (gc.worldSize / 2), (float)userSnake.body.Last<Vector2D>().Y + (gc.worldSize / 2));

            canvas.DrawImage(background, 0, 0, gc.worldSize, gc.worldSize);

            foreach (Power power in gc.world.Powerups.Values)
                DrawObjectWithTransform(canvas, power,
                  power.loc.X, power.loc.Y, 0,
                  drawPowerUp);

            foreach (Wall wall in gc.world.Walls.Values)
            {

                float begin;
                float end;
                List<Wall> view = gc.world.Walls.Values.ToList();
                if (wall.p1.X == wall.p2.X)
                {

                    if ((wall.p1.Y + (gc.worldSize / 2) < (wall.p2.Y + (gc.worldSize / 2))))
                    {
                        begin = (float)(wall.p1.Y + (gc.worldSize / 2));
                        end = (float)(wall.p2.Y + (gc.worldSize / 2));
                    }
                    else
                    {
                        end = (float)(wall.p1.Y + (gc.worldSize / 2));
                        begin = (float)(wall.p2.Y + (gc.worldSize / 2));

                    }

                    for (double i = begin - 25; i < end; i += 50)
                    {
                        canvas.DrawImage(this.wall, (float)(wall.p1.X + (gc.worldSize / 2) - 25), (float)i, 50, 50);

                    }


                }
                else
                {
                    if ((wall.p1.X + (gc.worldSize / 2) < (wall.p2.X + (gc.worldSize / 2))))
                    {
                        begin = (float)(wall.p1.X + (gc.worldSize / 2));
                        end = (float)(wall.p2.X + (gc.worldSize / 2));
                    }
                    else
                    {
                        end = (float)(wall.p1.X + (gc.worldSize / 2));
                        begin = (float)(wall.p2.X + (gc.worldSize / 2));

                    }

                    for (double i = begin - 25; i < end; i += 50)
                    {
                        canvas.DrawImage(this.wall, (float)i, (float)(wall.p1.Y + (gc.worldSize / 2) - 25), 50, 50);

                    }

                }




            }

            foreach (Snake snake in gc.world.Players.Values)
            {

                drawSnake(snake, canvas);

            }



        }



    }

    public void drawPowerUp(object o, ICanvas canvas)
    {
        Power power = o as Power;
        int width = 16;

        if (power.power % 2 == 0)
        {
            canvas.FillColor = Colors.DarkSalmon;
        }
        else
            canvas.FillColor = Colors.Firebrick;


        canvas.FillEllipse(-(width / 2), -(width / 2), width, width);
    }


    public void drawSnake(object o, ICanvas canvas)
    {
        Snake snake = o as Snake;
        int width = 10;

        List<Vector2D> list = snake.body;

        bool firstTimeThrough = false;
        Vector2D point1 = list[0];

        foreach (Vector2D v in list)
        {
            if (!firstTimeThrough)
            {
                firstTimeThrough = true;
                continue;
            }


            canvas.DrawLine((float)point1.X + (gc.worldSize / 2), (float)point1.Y + (gc.worldSize / 2), (float)v.X + (gc.worldSize / 2), (float)v.Y + (gc.worldSize / 2));

            point1 = v;

        }

    }


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
        canvas.Rotate((float)angle);
        drawer(o, canvas);

        // "pop" the transform
        canvas.RestoreState();
    }

}
