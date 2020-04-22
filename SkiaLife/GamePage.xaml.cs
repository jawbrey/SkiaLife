using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Timers;
using Amporis.Xamarin.Forms.ColorPicker;
using SkiaSharp;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SkiaLife
{
    public partial class GamePage : ContentPage, INotifyPropertyChanged
    {
        int rows;
        int cols;
        GridCell[,] gamedata;
        bool[,] nextdata;

        public int Generation { get; set; }
        public bool Paused { get; set; } = true;
        public string ButtonText { get; set; } = "Start";
        public Color BGColor { get; set; } = Color.White;
        public Color CellColor { get; set; } = Color.Black;
        public Color GridColor { get; set; } = Color.Green;

        private int time = 10;
        int CellSize = 30;


        public double Time
        {
            get
            {
                return (double)time;
            }
            set
            {
                time = (int)value;
            }
        }

        Timer timer;
        int timecount = 0;

        SKPaint gridpaint = new SKPaint()
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 5
        };

        SKPaint cellpaint = new SKPaint()
        {
            Style = SKPaintStyle.StrokeAndFill,
            StrokeWidth = 5
        };

        public GamePage()
        {
            InitializeComponent();

            BindingContext = this;

            timer = new Timer();
            timer.Elapsed += Timer_Elapsed;
            timer.Interval = 100;

            gridpaint.Color = ToSkiaColor(GridColor);
            cellpaint.Color = ToSkiaColor(CellColor);

        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timecount++;

            if ( (timecount % (11 - (int)Time)) == 0)
            {
                Generation++;

                MainThread.BeginInvokeOnMainThread(() =>
               {
                   UpdateGrid();
               });
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            rows = (int) (GameGrid.CanvasSize.Height / CellSize);
            cols = (int) (GameGrid.CanvasSize.Width / CellSize);

            gamedata = new GridCell[rows, cols];
            nextdata = new bool[rows, cols];
            
            GameGrid.InvalidateSurface();
        }

        private int Neighbors(int r, int c)
        {
            int count = 0;

            try
            { 
            if (r > 0 && c > 0 && gamedata[r - 1, c - 1].Alive) count++;
                if (r > 0 && gamedata[r - 1, c].Alive) count++;
                if (r > 0 && c < (cols-1) && gamedata[r - 1, c + 1].Alive) count++;

                if (c > 0 && gamedata[r, c - 1].Alive) count++;
                if (c < (cols-1) && gamedata[r, c + 1].Alive) count++;

                if (r < rows && c > 0 && gamedata[r + 1, c - 1].Alive) count++;
                if (r < rows && gamedata[r + 1, c].Alive) count++;
                if (r < rows && c < (cols-1) && gamedata[r + 1, c + 1].Alive) count++;
            } catch (Exception ex)
            {

            }

            return count;
        }

        private void UpdateGrid()
        {
            
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    var n = Neighbors(r, c);

                    if (n == 0 || n ==1 || n >= 4)
                    {
                        nextdata[r, c] = false;
                    }
                    if (gamedata[r, c].Alive && (n == 2 || n == 3))
                    {
                        nextdata[r, c] = true;
                    }
                    if (!gamedata[r, c].Alive && n == 3)
                    {
                        nextdata[r, c] = true;
                    }
                }
            }

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    gamedata[r, c].Alive = nextdata[r, c];
                }
            }

            GameGrid.InvalidateSurface();

        }

        void GameGrid_PaintSurface(System.Object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            var h = info.Height;
            var w = info.Width;



            for (int y = 0; y < h; y += CellSize)
            {
                canvas.DrawLine(0, y, w, y, gridpaint);
            }

            for (int x = 0; x < w; x += CellSize)
            {
                canvas.DrawLine(x, 0, x, h, gridpaint);
            }

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (gamedata[r, c] == null) gamedata[r, c] = new GridCell();

                    if (gamedata[r, c].Alive)
                    {
                        canvas.DrawCircle((c * CellSize) + (CellSize/2), (r * CellSize) + (CellSize/2), (CellSize/2 - 5), cellpaint);
                    }
                }
            }

        }

        void TouchEffect_TouchAction(System.Object sender, TouchTracking.TouchActionEventArgs args)
        {
            if (args.Type == TouchTracking.TouchActionType.Cancelled ||
                args.Type == TouchTracking.TouchActionType.Exited ||
                args.Type == TouchTracking.TouchActionType.Released) return;

            Point pt = args.Location;
            SKPoint point =
                new SKPoint((float)(GameGrid.CanvasSize.Width * pt.X / GameGrid.Width),
                            (float)(GameGrid.CanvasSize.Height * pt.Y / GameGrid.Height));

            var x = point.X;
            var y = point.Y;

            var row = (int) y / CellSize;
            var col = (int)x / CellSize;

            if (row >= rows) return;

            if (gamedata[row, col] == null) gamedata[row, col] = new GridCell();
            gamedata[row, col].Tapped(args.Id);

            GameGrid.InvalidateSurface();
        }

        void Next_Clicked(System.Object sender, System.EventArgs e)
        {
            Generation++;
            UpdateGrid();
        }

        void Clear_Clicked(System.Object sender, System.EventArgs e)
        {
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    gamedata[r, c].Alive = false;
                }
            }

            timecount = 0;
            Generation = 0;
            GameGrid.InvalidateSurface();
        }

        void Start_Clicked(System.Object sender, System.EventArgs e)
        {
            if (Paused)
            {
                Paused = false;
                timer.Start();
                ButtonText = "Pause";
            }
            else
            {
                Paused = true;
                timer.Stop();
                ButtonText = "Start";
            }
            
        }

        async void BGClicked(System.Object sender, System.EventArgs e)
        {
            var color = await ColorPickerDialog.Show(OuterGrid, "Background Color", BGColor, null);

            if (color != null) BGColor = color;
        }

        async void GridClicked(System.Object sender, System.EventArgs e)
        {
            var color = await ColorPickerDialog.Show(OuterGrid, "Grid Color", GridColor, null);

            if (color != null)
            {
                GridColor = color;
                gridpaint.Color = ToSkiaColor(color);
                GameGrid.InvalidateSurface();
            }

        }

        async void CellClicked(System.Object sender, System.EventArgs e)
        {
            var color = await ColorPickerDialog.Show(OuterGrid, "Cell Color", CellColor, null);

            if (color != null)
            {
                CellColor = color;
                cellpaint.Color = ToSkiaColor(color);
                GameGrid.InvalidateSurface();
            }
        }

        SKColor ToSkiaColor(Xamarin.Forms.Color color)
        {
            var r = (byte)(color.R * 255);
            var g = (byte)(color.G * 255);
            var b = (byte)(color.B * 255);
            var a = (byte)(color.A * 255);

            return new SKColor(r, g, b, a);
        }
    }
}
