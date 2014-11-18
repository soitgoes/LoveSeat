using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using FluentAssertions;
using LoveSeat.Cloudant;
using LoveSeat.Support;
using Moq;
using Moq.AutoMock;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace LoveSeat.IntegrationTest.Unit
{
    [TestFixture]
    public class CloudantDatabaseTest
    {
        [Test]
        public void TestGetCloudantSecurityDocument_Should_Return_A_Valid_Document()
        {
            //SETUP
            var mocker = new AutoMocker();

            //connection returns base uri
            mocker.Setup<ICouchConnection>(c => c.BaseUri).Returns(new Uri("http://localhost"));

            //GetCouchResponse returns security doc as per Cloudant documentation
            const string responseString = "{ " +
                                          "\"_id\":\"_security\"," +
                                          "\"cloudant\":" +
                                          "{" +
                                          "  \"userwithreadonlyaccess\": [" +
                                          "    \"_reader\"" +
                                          "  ]," +
                                          "  \"userwithreadandwriteaccess\": [" +
                                          "    \"_reader\"," +
                                          "    \"_writer\"" +
                                          "  ]" +
                                          "}," +
                                          "\"ok\":true" +
                                          "}";
            mocker.Setup<ICouchResponse>(r => r.ResponseString).Returns(responseString);
            mocker.Setup<ICouchResponse, HttpStatusCode>(r => r.StatusCode).Returns(HttpStatusCode.OK);
            mocker.Setup<ICouchRequest>(r => r.GetCouchResponse()).Returns(mocker.Get<ICouchResponse>());
            

            //couchFactory returns request
            mocker.Setup<ICouchFactory>(f => f.GetRequest(It.IsAny<Uri>(), It.IsAny<ICouchConnection>()))
                .Returns(mocker.Get<ICouchRequest>());
            //Get request returns request
            mocker.Setup<ICouchRequest>(r => r.Get()).Returns(mocker.Get<ICouchRequest>());
            //Json content type returns request
            mocker.Setup<ICouchRequest>(r => r.Json()).Returns(mocker.Get<ICouchRequest>());

            //TEST
            var database = mocker.CreateInstance<CloudantDatabase>();

            var securityDocument = (CloudantSecurityDocument)database.GetSecurityDocument();

            securityDocument.Id.Should().Be("_security");
            securityDocument.CloudantSecuritySection.GetAssignment("userwithreadonlyaccess").Should().BeEquivalentTo("_reader");
            securityDocument.CloudantSecuritySection.GetAssignment("userwithreadandwriteaccess").Should().BeEquivalentTo("_reader", "_writer");
            securityDocument.Ok.Should().BeTrue();

            mocker.VerifyAll();
        }

        [Test]
        public void TestUpdateSecurityDoc_Should_Succeed_For_Valid_Document()
        {
            //SETUP
            var mocker = new AutoMocker();

            //connection returns base uri
            mocker.Setup<ICouchConnection>(c => c.BaseUri).Returns(new Uri("http://localhost"));

            //GetCouchResponse returns security doc as per Cloudant documentation
            const string responseString = "{ " +
                                          "\"ok\":true" +
                                          "}";
            mocker.Setup<ICouchResponse>(r => r.ResponseString).Returns(responseString);
            mocker.Setup<ICouchResponse, HttpStatusCode>(r => r.StatusCode).Returns(HttpStatusCode.OK);
            mocker.Setup<ICouchRequest>(r => r.GetCouchResponse()).Returns(mocker.Get<ICouchResponse>());

            //couchFactory returns request
            mocker.Setup<ICouchFactory>(f => f.GetRequest(It.IsAny<Uri>(), It.IsAny<ICouchConnection>()))
                .Returns(mocker.Get<ICouchRequest>());
            //Get request returns request
            mocker.Setup<ICouchRequest>(r => r.Put()).Returns(mocker.Get<ICouchRequest>());
            //Json content type returns request
            mocker.Setup<ICouchRequest>(r => r.Json()).Returns(mocker.Get<ICouchRequest>());
            //Data returns request
            mocker.Setup<ICouchRequest>(r => r.Data(It.IsAny<string>())).Returns(mocker.Get<ICouchRequest>());

            //security doc 
            var securityDoc = new CloudantSecurityDocument();
            securityDoc.CloudantSecuritySection.AddUser("user1", "_reader");
            securityDoc.CloudantSecuritySection.AddUser("user2", "_reader","_writer");

            //TEST
            var database = mocker.CreateInstance<CloudantDatabase>();

            database.UpdateSecurityDocument(securityDoc);

            mocker.VerifyAll();
        }
    }
}
