using System;
using System.Diagnostics.CodeAnalysis;
using URSA.Web.Http;
using URSA.Web.Http.Mapping;
using URSA.Web.Mapping;

namespace URSA.Web.Tests
{
    [ExcludeFromCodeCoverage]
    [Route("api/test")]
    public class TestController : IController
    {
        public ResponseInfo Response { get; set; }

        IResponseInfo IController.Response { get { return Response; } set { Response = (ResponseInfo)value; } }

        public int Add(int operandA = 0, int operandB = 0)
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

        public double Power([FromUri] double operandA = 0.0, [FromUri] double operandB = 0.0)
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

        public void GetByGuid(Guid id, out Guid newId)
        {
            newId = default(Guid);
        }

        public void GetBySomeId(int id, out int key)
        {
            key = default(int);
        }

        public object GetSomething(object instance)
        {
            return null;
        }

        public int Whatever(out int another, out int key)
        {
            another = 1;
            key = 2;
            return 3;
        }
    }
}