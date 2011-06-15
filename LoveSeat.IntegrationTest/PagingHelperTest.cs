using LoveSeat.Interfaces;
using LoveSeat.Support;
using Moq;
using NUnit.Framework;

namespace LoveSeat.IntegrationTest
{
    [TestFixture]
    public class PagingHelperTest
    {
        [Test]
        public void Should_Show_Previous_If_OffSet_Not_Equal_Zero()
        {
            var result = new Mock<IViewResult>();
            result.ExpectGet(x => x.OffSet).Returns(1);
            var options = new ViewOptions();
            var model = new PageableModel();
            model.UpdatePaging(options, result.Object);
             Assert.IsTrue(model.ShowPrev);        
        }
        [Test]
        public void Should_Not_Show_Previous_If_Offset_Equal_Zero()
        {
            var result = new Mock<IViewResult>();
            result.ExpectGet(x => x.OffSet).Returns(0);
            var options = new ViewOptions();
            var model = new PageableModel();
            model.UpdatePaging(options, result.Object);
            Assert.IsFalse(model.ShowPrev);        
        }
        [Test]
        public void Should_Show_Next_Unless_Offset_Plus_Limit_Gte_Count()
        {
            var result = new Mock<IViewResult>();
            result.ExpectGet(x => x.OffSet).Returns(5);
            var options = new ViewOptions();
            options.Limit = 5;
            result.ExpectGet(x => x.TotalRows).Returns(10);
            var model = new PageableModel();
            model.UpdatePaging(options, result.Object);
            Assert.IsFalse(model.ShowNext);
        }
    }
}