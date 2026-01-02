using Raylib_cs;
using System.Numerics;


namespace fisica_sim;

    static class Program
    {
        private const float Gravedad = 500.0f;
        private const float RozAire = 0.999f;
        private const float Restitucion = 0.8f;
        private const int Width = 1024;
        private const int Height = 720;

        static void Main()
        {
            // Configuración ventana
            Raylib.InitWindow(Width, Height, "Física - Lanzar con ratón");
            Raylib.SetTargetFPS(60);
            //Para los errores de novato he hecho uso de Gemini 3 Pro. CClaude no me dejaba seguir
            // Las pelotas tienen que declararse fuera del bucle principal. Error de novato...
            List<Pelota> pelotas = new List<Pelota>();
            // Código generado por Gemini 3 Pro empieza aquí
            Random rnd = new Random();
            int numPelotas = 5;

            // Generar 10 pelotas (o las que quieras)
            for (int i = 0; i < numPelotas; i++)
            {
                float rX = rnd.Next(50, Width - 50); // Posición X aleatoria
                float rY = rnd.Next(50, Height - 200); // Posición Y aleatoria
                float radio = rnd.Next(10, 50); // Radio aleatorio entre 15 y 35
                float vx = rnd.Next(100 - Width, Width - 100); // velocidad x aleatoria entre 50 - Width y Width -50
                float vy = rnd.Next(100 - Height, Height - 100); // velocidad y aleatoria entre 50 - Height y Height -50

                // Generamos un color aleatorio
                Color colorAleatorio = new Color(rnd.Next(256), rnd.Next(256), rnd.Next(256), 255);

                pelotas.Add(new Pelota(rX, rY, vx, vy, radio, colorAleatorio));
            }

            // El código generado acaba aquí. He añadido vx y vy para añadirles una velocidad aleatoria en función de
            // las dimensiones de la pantalla.
            while (!Raylib.WindowShouldClose())
            {
                float dt = Raylib.GetFrameTime();
                Vector2 mousePosicion = Raylib.GetMousePosition();

                // Colisiones entre pelotas (Bucle anidado)
                // Usamos for en lugar de foreach para tener control sobre los índices y no repetir parejas
                // Colisiones entre pelotas (Bucle anidado)
                for (int i = 0; i < pelotas.Count; i++)
                {
                    for (int j = i + 1; j < pelotas.Count; j++)
                    {
                        // Solo detectar colisiones si NINGUNA está siendo arrastrada
                        if (!pelotas[i].Arrastrando && !pelotas[j].Arrastrando)
                        {
                            pelotas[i].DetectarColisiones(pelotas[j], Restitucion);
                        }
                    }
                }

                foreach (Pelota pelota in pelotas)
                {
                    // 1. LÓGICA / INPUT
                    if (pelota.DetectarClick(mousePosicion)) pelota.IniciarArrastre();

                    if (pelota.Arrastrando)
                    {
                        if (Raylib.IsMouseButtonReleased(MouseButton.Left))
                        {
                            pelota.SoltarArrastre(mousePosicion);
                        }
                        // AQUÍ BORRAMOS el "else { pelota1.Arrastrar... }"
                        // porque Arrastrar contiene código de dibujo y este es el bloque de lógica.
                    }
                    else
                    {
                        pelota.DetectarParedes(Width, Height, Restitucion);

                        pelota.ActualizarFisica(dt, Gravedad, RozAire);
                    }
                }

                // 2. DIBUJO
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);

                // Al llamar a Dibujar aquí, dentro ya se llama a "Arrastrar" si es necesario,
                // y como estamos dentro de BeginDrawing, las líneas se verán perfectas. Otro error de novato...
                foreach (Pelota pelota in pelotas)
                {
                    pelota.Dibujar(mousePosicion);
                }

                Raylib.DrawText("Click y arrastra la pelota", 10, 10, 20, Color.White);
                Raylib.EndDrawing();

            }

            Raylib.CloseWindow();
        }
    }
