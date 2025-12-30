using Raylib_cs;
using System.Numerics;

class Program
{
    static void Main()
    {
        // Configuración ventana
        Raylib.InitWindow(800, 600, "Física - Caída libre");
        Raylib.SetTargetFPS(60);

        // Estado de la pelota
        Vector2 posicion = new Vector2(400, 50);
        Vector2 velocidad = new Vector2(500, 250);
        float gravedad = 500.0f; // píxeles/s²
        float radio = 20.0f;
        float restitucion = 0.9999f;
        float rozAire = 0.999f;

        while (!Raylib.WindowShouldClose())
        {
            float dt = Raylib.GetFrameTime();

            // Física
            velocidad.Y += gravedad * dt;
            velocidad *= MathF.Pow(rozAire, dt);
            posicion += velocidad * dt;

            // Rebote en paredes laterales
            if (posicion.X + radio > 800)
            {
                posicion.X = 800 - radio;
                velocidad.X = -velocidad.X * restitucion; // coeficiente de restitución
            }

            if (posicion.X - radio < 0)
            {
                posicion.X = radio;
                velocidad.X = -velocidad.X * restitucion;
            }

            // Rebote en el suelo
            if (posicion.Y + radio > 600)
            {
                posicion.Y = 600 - radio;
                velocidad.Y = -velocidad.Y * restitucion; // coeficiente de restitución
            }

            if (posicion.Y - radio < 0)
            {
                posicion.Y = radio;
                velocidad.Y = -velocidad.Y * restitucion;
            }

            // Dibujar
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);
            Raylib.DrawCircleV(posicion, radio, Color.Red);
            Raylib.DrawText("Presiona ESC para salir", 10, 10, 20, Color.White);
            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}