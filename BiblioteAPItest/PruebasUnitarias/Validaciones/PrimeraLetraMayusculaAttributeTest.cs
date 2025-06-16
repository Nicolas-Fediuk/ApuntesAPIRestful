using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebApiAutores.Validaciones;

namespace BiblioteAPItest.PruebasUnitarias.Validaciones
{
    [TestClass]
    public class PrimeraLetraMayusculaAttributeTest
    {
        [TestMethod]
        //distintos valores del value, correo 3 veces, para cada valor
        [DataRow("")]
        [DataRow("    ")]
        [DataRow(null)]
        [DataRow("Nicolas")]
        //Metodo a testear, lo que espero que ocurra, bajo que condicion
        public void IsValid_RetornaExistoso_SiValueNoTieneLaPrimeraLetraMinuscula(string value)
        {
            // Preparacion 

            var primeraLetraMayusculaAttribute = new PrimeraLetraMayusculaAttribute();
            var validationC0ntext = new ValidationContext(new object());

            //Prueba

            var resultado = primeraLetraMayusculaAttribute.GetValidationResult(value, validationC0ntext);


            // Verificacion

            //que los dos valores sean iguales
            Assert.AreEqual(expected: ValidationResult.Success, actual: resultado); 
        }


        [TestMethod]
        [DataRow("nicolas")]
        //Metodo a testear, lo que espero que ocurra, bajo que condicion
        public void IsValid_RetornaExistoso_SiValueTieneLaPrimeraLetraMinuscula(string value)
        {
            // Preparacion 

            var primeraLetraMayusculaAttribute = new PrimeraLetraMayusculaAttribute();
            var validationC0ntext = new ValidationContext(new object());

            //Prueba

            var resultado = primeraLetraMayusculaAttribute.GetValidationResult(value, validationC0ntext);


            // Verificacion

            //que los dos valores sean iguales
            Assert.AreEqual(expected: ValidationResult.Success, actual: resultado.ErrorMessage);
        }
    }
}
