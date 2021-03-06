﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using URSA.Web.Http;

namespace Given_instance_of_the
{
    [TestClass]
    public class Exception_extension
    {
        [TestMethod]
        public void it_should_throw_when_no_exception_is_passed_for_HttpException_conversion()
        {
            ((Exception)null).Invoking(instance => instance.AsHttpException()).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("exception");
        }

        [TestMethod]
        public void it_should_leave_ProtocolException_as_is()
        {
            var exception = new ProtocolException(HttpStatusCode.Conflict);

            exception.AsHttpException().Should().Be(exception);
        }

        [TestMethod]
        public void it_should_use_a_status_code_from_a_HttpException()
        {
            new HttpException((int)HttpStatusCode.Conflict, "test").AsHttpException().Status.Should().Be(HttpStatusCode.Conflict);
        }

        [TestMethod]
        public void it_should_throw_when_no_exception_is_passed_for_status_code_conversion()
        {
            ((Exception)null).Invoking(instance => instance.ToHttpStatusCode()).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("exception");
        }

        [TestMethod]
        public void it_should_throw_when_no_exception_type_is_passed_for_status_code_conversion()
        {
            ((Type)null).Invoking(instance => instance.ToHttpStatusCode()).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("exceptionType");
        }

        [TestMethod]
        public void it_should_throw_when_a_type_passed_for_status_code_conversion_is_not_an_exception()
        {
            ((Type)null).Invoking(instance => typeof(int).ToHttpStatusCode()).ShouldThrow<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("exceptionType");
        }

        [TestMethod]
        public void it_should_map_an_exception_directly()
        {
            typeof(ArgumentNullException).ToHttpStatusCode().Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public void it_should_map_an_exception_indirectly()
        {
            typeof(EncoderFallbackException).ToHttpStatusCode().Should().Be(HttpStatusCode.BadRequest);
        }

        [TestMethod]
        public void it_should_fallback_to_Internal_server_error()
        {
            typeof(KeyNotFoundException).ToHttpStatusCode().Should().Be(HttpStatusCode.InternalServerError);
        }

        [TestMethod]
        public void it_should_throw_when_no_exception_type_name_is_provided()
        {
            ((string)null).Invoking(instance => instance.ToHttpStatusCode()).ShouldThrow<ArgumentNullException>().Which.ParamName.Should().Be("exceptionTypeName");
        }

        [TestMethod]
        public void it_should_throw_when_an_exception_type_name_provided_is_empty()
        {
            String.Empty.Invoking(instance => instance.ToHttpStatusCode()).ShouldThrow<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("exceptionTypeName");
        }

        [TestMethod]
        public void it_should_throw_when_an_exception_type_name_provided_has_no_Exception_word()
        {
            "test".Invoking(instance => instance.ToHttpStatusCode()).ShouldThrow<ArgumentOutOfRangeException>().Which.ParamName.Should().Be("exceptionTypeName");
        }

        [TestMethod]
        public void it_should_map_an_exception_type_name_directly()
        {
            typeof(ArgumentNullException).FullName.ToHttpStatusCode().Should().Be(HttpStatusCode.BadRequest);
        }
    }
}