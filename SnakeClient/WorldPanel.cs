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



namespace SnakeGame;
public class WorldPanel : StackLayout,  IDrawable
{
    private IImage wall;
    private IImage background;
    private GraphicsView graphicsView;

    private bool initializedForDrawing = false;


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
        wall = loadImage( "wallsprite.png" );
        background = loadImage( "background.png" );
        initializedForDrawing = true;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if ( !initializedForDrawing )
            InitializeDrawing();

        // undo previous transformations from last frame
        canvas.ResetState();

        // example code for how to draw
        // (the image is not visible in the starter code)
        if( gc.world != null ) 
        {        
            canvas.DrawImage(background, gc.world.size, gc.world.size, background.Width, background.Height);
        }

        canvas.DrawImage(wall, 200, 300, wall.Width, wall.Height);
    }


    public void setGameController(GameController.GameController controller)
    {
        this.gc = controller;
    }

    public void setGraphicsView(GraphicsView graphicsView) 
    { 
      this.graphicsView = graphicsView;
    }

}
