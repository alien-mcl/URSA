using System;
using URSA.Web.Http;
using URSA.Web.Http.Mapping;
using URSA.Web.Mapping;

namespace URSA.Web.Tests
{
    [Route("api/test")]
    public class TestController : IController
    {
        public ResponseInfo Response { get; set; }

        IResponseInfo IController.Response { get { return Response; } set { Response = (ResponseInfo)value; } }

        public int Add(int operandA, int operandB)
        {
            return operandA + operandB;
        }

        [Route("sub")]
        public int Substract([FromUri] int operandA, [FromQueryString] int operandB)
        {
            return operandA - operandB;
        }

        [return: ToHeader("Pragma")]
        public int Multiply([FromQueryString] int operandA, [FromUri] int operandB)
        {
            return operandA * operandB;
        }

        [Route("div")]
        [OnPost]
        [return: AsMediaType("text/plain")]
        public int Divide([FromUri] int operandA, [FromBody] int operandB)
        {
            return operandA / operandB;
        }

        [return: AsMediaType("text/plain")]
        public int PostModulo([FromBody] int operandA, [FromBody] int operandB)
        {
            return operandA % operandB;
        }

        public double Power([FromUri] double operandA, [FromUri] double operandB)
        {
            return Math.Pow(operandA, operandB);
        }

        public double Log(double[] operands)
        {
            if (operands.Length == 0)
            {
                throw new ArgumentOutOfRangeException("operands");
            }

            if (operands.Length == 1)
            {
                return Math.Log(operands[0]);
            }

            return Math.Log(operands[0], operands[1]);
        }

        public void PostString([FromBody] string data)
        {
        }

        public void PostStrings([FromBody] string[] data)
        {
        }

        public void PostForm([FromBody] string key, [FromBody] int value)
        {
        }

        [OnPost]
        public void Upload([FromBody] string fileName, [FromBody] byte[] data)
        {
        }
    }
}