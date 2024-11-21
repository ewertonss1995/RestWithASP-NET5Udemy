using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestWithASPNETUdemy.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class CalculatorController : ControllerBase
    {
        private readonly ILogger<CalculatorController> _logger;

        public CalculatorController(ILogger<CalculatorController> logger)
        {
            _logger = logger;
        }

        [HttpGet("sum/{firstNumber}/{secondNumber}")]
        public IActionResult GetSum(string firstNumber, string secondNumber)
        {
            if (IsNumeric(firstNumber) && IsNumeric(secondNumber))
            {
                var result = convertToDecimal(firstNumber) + convertToDecimal(secondNumber);
                return Ok(result.ToString());
            }

            return BadRequest("Invalid Input.");
        }

        [HttpGet("sub/{firstNumber}/{secondNumber}")]
        public IActionResult GetSubtraction(string firstNumber, string secondNumber)
        {
            if (IsNumeric(firstNumber) && IsNumeric(secondNumber))
            {
                var result = convertToDecimal(firstNumber) - convertToDecimal(secondNumber);
                return Ok(result.ToString());
            }

            return BadRequest("Invalid Input.");
        }

        [HttpGet("mult/{firstNumber}/{secondNumber}")]
        public IActionResult GetMultiplication(string firstNumber, string secondNumber)
        {
            if (IsNumeric(firstNumber) && IsNumeric(secondNumber))
            {
                var result = convertToDecimal(firstNumber) * convertToDecimal(secondNumber);
                return Ok(result.ToString());
            }

            return BadRequest("Invalid Input.");
        }

        [HttpGet("div/{firstNumber}/{secondNumber}")]
        public IActionResult GetDivision(string firstNumber, string secondNumber)
        {
            if (IsNumeric(firstNumber) && IsNumeric(secondNumber))
            {
                var result = convertToDecimal(firstNumber) / convertToDecimal(secondNumber);
                return Ok(result.ToString());
            }

            return BadRequest("Invalid Input.");
        }

        [HttpGet("mean/{firstNumber}/{secondNumber}")]
        public IActionResult GetMean(string firstNumber, string secondNumber)
        {
            if (IsNumeric(firstNumber) && IsNumeric(secondNumber))
            {
                var result = (convertToDecimal(firstNumber) + convertToDecimal(secondNumber)) / 2;
                return Ok(result.ToString());
            }

            return BadRequest("Invalid Input.");
        }

        [HttpGet("sqrt/{number}")]
        public IActionResult GetSquareRoot(string number)
        {
            if (IsNumeric(number))
            {
                var decimalValue = convertToDecimal(number);
                if (decimalValue < 0)
                {
                    return BadRequest("Cannot calculate square root of a negative number.");
                }

                var result = Math.Sqrt((double)decimalValue);
                return Ok(result.ToString());
            }

            return BadRequest("Invalid Input.");
        }

        private bool IsNumeric(string stringNumber)
        {
            double number;
            bool isNumber = double.TryParse(stringNumber,
            System.Globalization.NumberStyles.Any,
            System.Globalization.NumberFormatInfo.InvariantInfo,
            out number);

            return isNumber;
        }

        private decimal convertToDecimal(string stringNumber)
        {
            decimal decimalValue;

            if (decimal.TryParse(stringNumber, out decimalValue))
            {
                return decimalValue;
            }

            return 0;
        }
    }
}