using Raylib_cs;
using System.Numerics;


namespace fisica_sim;

public class Pelota
{
    private Vector2 _posicion;
    private Vector2 _velocidad;
    private readonly float _radio;
    private readonly Color _color;
    public bool Arrastrando;
    private Vector2 _posicionInicial;
    private readonly float _masa;

    public Pelota(float posicionX, float posicionY, float velocidadX, float velocidadY, float radio, Color color)
    {
        _posicion.X = posicionX;
        _posicion.Y = posicionY;
        _velocidad.X = velocidadX;
        _velocidad.Y = velocidadY;
        _radio = radio;
        _masa = radio * radio;
        _color = color;
    }

    public  bool DetectarClick(Vector2 mousePosicion)
    {
        if (!Raylib.IsMouseButtonPressed(MouseButton.Left)) return false;
        float distancia = Vector2.Distance(mousePosicion, _posicion);
        if (distancia <= _radio)
        {
            return true;
        }

        return false;

    }

    public void IniciarArrastre()
    {
            Arrastrando = true;
            _posicionInicial = _posicion;
            _velocidad = Vector2.Zero;
    }

    private void Arrastrar(Vector2 mousePosicion)
    {

        // Vector desde inicio hasta ratón
        Vector2 deltaRaton = mousePosicion - _posicionInicial;

        // Línea amarilla: desde inicio hasta ratón
        Raylib.DrawLineEx(_posicionInicial, mousePosicion, 2.0f, Color.Yellow);

        // Calcular ángulo entre 0° y 90° (valor absoluto)
        float anguloAbsoluto = MathF.Atan2(MathF.Abs(deltaRaton.Y), MathF.Abs(deltaRaton.X));
        float anguloGrados = anguloAbsoluto * (180.0f / MathF.PI);

        // Dibujar línea horizontal
        Vector2 horizontal = _posicionInicial + new Vector2(deltaRaton.X, 0);
        Raylib.DrawLineEx(_posicionInicial, horizontal, 2.0f, Color.White);

        // Dibujar sector circular
        float radioArco = MathF.Min(MathF.Abs(deltaRaton.X), _radio);

        int numLineas = 30;

        // Ángulo base de la horizontal según el cuadrante
        float anguloHorizontal;
        if (deltaRaton.X >= 0)
            anguloHorizontal = 0; // Horizontal derecha
        else
            anguloHorizontal = MathF.PI; // Horizontal izquierda

        // Ángulo de la línea amarilla
        float anguloLinea = deltaRaton.X switch
        {
            >= 0 when deltaRaton.Y < 0 => -anguloAbsoluto,
            < 0 when deltaRaton.Y < 0 => MathF.PI + anguloAbsoluto,
            < 0 when deltaRaton.Y >= 0 => MathF.PI - anguloAbsoluto,
            _ => anguloAbsoluto
        };

        // Dibujar el arco desde horizontal hasta línea
        Vector2 puntoAnterior = _posicionInicial + new Vector2(
            MathF.Cos(anguloHorizontal) * radioArco,
            MathF.Sin(anguloHorizontal) * radioArco
        );

        if (MathF.Abs(deltaRaton.X) > 0.1f)
        {
            for (int i = 1; i <= numLineas; i++)
            {
                float t = (float)i / numLineas;
                float anguloActual = anguloHorizontal + t * (anguloLinea - anguloHorizontal);

                Vector2 puntoActual = _posicionInicial + new Vector2(
                    MathF.Cos(anguloActual) * radioArco,
                    MathF.Sin(anguloActual) * radioArco
                );

                Raylib.DrawLineV(puntoAnterior, puntoActual, Color.DarkGray);
                puntoAnterior = puntoActual;
            }
        }

        // Mostrar información
        string textoAngulo = $"Ángulo: {anguloGrados:F1}°";
        Raylib.DrawText(textoAngulo, 10, 40, 20, Color.DarkGray);

        Raylib.DrawCircleV(mousePosicion, 5.0f, Color.Red);
    }

    public void SoltarArrastre(Vector2 mousePosicion)
    {
        Arrastrando = false;
        Vector2 delta = mousePosicion - _posicionInicial;
        _velocidad = delta * 10000.0f / _masa;
    }

    public void ActualizarFisica(float dt, float gravedad, float rozAire)
    {
        _velocidad.Y += gravedad * dt;
        _velocidad *= MathF.Pow(rozAire, dt);
        _posicion += _velocidad * dt;

        // Amortiguamiento extra para velocidades bajas
        if (_velocidad.Length() < 50.0f)
        {
            _velocidad *= 0.95f; // Fricción adicional cuando va lento
        }

        // Detener completamente si la velocidad es insignificante
        if (_velocidad.Length() < 1.5f)
        {
            _velocidad = Vector2.Zero;
        }
    }

    public void DetectarParedes(int anchoVentana, int altoVentana, float restitucion)
    {
        // Rebotes
        if (_posicion.X + _radio > anchoVentana)
        {
            _posicion.X = anchoVentana - _radio;
            _velocidad.X = -_velocidad.X * restitucion;
        }
        if (_posicion.X - _radio < 0)
        {
            _posicion.X = _radio;
            _velocidad.X = -_velocidad.X * restitucion;
        }
        if (_posicion.Y + _radio > altoVentana)
        {
            _posicion.Y = altoVentana - _radio;
            _velocidad.Y = -_velocidad.Y * restitucion;
        }
        if (_posicion.Y - _radio < 0)
        {
            _posicion.Y = _radio;
            _velocidad.Y = -_velocidad.Y * restitucion;
        }
    }

    // Este método ha sido diseñado con ayuda de Gemini 3 Pro. Probablemente, habría obtenido una forma más difícil de
    // obtenerlo, pero no sabía cómo acceder a los elementos de una list... :(
    public void DetectarColisiones(Pelota otraPelota, float restitucion)
    {
        Vector2 vecDistPelotas = otraPelota._posicion - this._posicion;
        float disPelotasSq = vecDistPelotas.LengthSquared();
        float sumaRadios = this._radio + otraPelota._radio;

        if (disPelotasSq < sumaRadios * sumaRadios)
        {
            float distancia = MathF.Sqrt(disPelotasSq);
            if (distancia == 0) return;

            Vector2 normal = vecDistPelotas / distancia;

            // 1. RESOLUCIÓN ESTÁTICA (con margen extra)
            float solapamiento = sumaRadios - distancia;
            float margenExtra = 0.1f; // Pequeño margen para evitar vibraciones
            solapamiento += margenExtra;

            float masaTotal = this._masa + otraPelota._masa;
            float ratio1 = otraPelota._masa / masaTotal;
            float ratio2 = this._masa / masaTotal;

            this._posicion -= normal * (solapamiento * ratio1);
            otraPelota._posicion += normal * (solapamiento * ratio2);

            // 2. RESOLUCIÓN DINÁMICA
            Vector2 velocidadRelativa = this._velocidad - otraPelota._velocidad;
            float velNormal = Vector2.Dot(velocidadRelativa, normal);

            // Solo aplicar impulso si se están acercando significativamente
            if (velNormal > -0.5f) return; // Umbral de 0.5 píxeles/segundo

            float impulsoEscalar = -(1 + restitucion) * velNormal;
            impulsoEscalar /= (1 / this._masa + 1 / otraPelota._masa);

            Vector2 impulsoVector = impulsoEscalar * normal;

            if (MathF.Abs(velNormal) < 5.0f)
            {
                // Colisión lenta: amortiguar velocidades
                this._velocidad *= 0.95f;
                otraPelota._velocidad *= 0.95f;
            }

            this._velocidad += impulsoVector / this._masa;
            otraPelota._velocidad -= impulsoVector / otraPelota._masa;
            // 3. FRICCIÓN TANGENCIAL
            Vector2 tangente = new Vector2(-normal.Y, normal.X); // Perpendicular a la normal
            float velTangencial = Vector2.Dot(velocidadRelativa, tangente);

            // Coeficiente de fricción (0 = sin fricción, 1 = fricción total)
            float coefFriccion = 0.7f;

            // Impulso de fricción (limitado por el coeficiente)
            float impulsoFriccion = -velTangencial * coefFriccion;
            impulsoFriccion /= (1 / this._masa + 1 / otraPelota._masa);

            Vector2 impulsoFriccionVector = impulsoFriccion * tangente;

            this._velocidad += impulsoFriccionVector / this._masa;
            otraPelota._velocidad -= impulsoFriccionVector / otraPelota._masa;
        }
    }

    public void Dibujar(Vector2 mousePosicion)
    {


            if (Arrastrando)
            {
                Arrastrar(mousePosicion);
            }

            Raylib.DrawCircleV(_posicion, _radio, _color);

            // Vector velocidad
            if (_velocidad.Length() > 1.0f)
            {
                Vector2 puntoFinal = _posicion + _velocidad * 0.1f;
                Raylib.DrawLineEx(_posicion, puntoFinal, 3.0f, Color.Blue);
                Raylib.DrawCircleV(puntoFinal, 4.0f, Color.Blue);
            }


    }
}
