using Microsoft.Extensions.Options;

namespace WebApiAutores
{
    public class PagosProcesamiento
    {
        private TarifasOpciones _tarifaOpciones;

        public PagosProcesamiento(IOptionsMonitor<TarifasOpciones> opcionsTarifa)
        {
            this._tarifaOpciones = opcionsTarifa.CurrentValue;

            opcionsTarifa.OnChange(nuevaTarifa =>
            {
                Console.WriteLine("tarifa actualizada");
                _tarifaOpciones = nuevaTarifa;
            });
        }

        public void ProcesarPago()
        {
            //aqui usamos 
        }

        public TarifasOpciones ObtenerTarifas()
        {
            return _tarifaOpciones;
        }
    }
}
