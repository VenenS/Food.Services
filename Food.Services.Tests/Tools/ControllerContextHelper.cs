namespace Food.Services.Tests.Tools
{
    class ControllerContextHelper
    {/*
       internal static ControllerContext Setup(long userId, string email = "default", string role = EnumUserRole.Admin)
        {
            var identity = new ClaimsIdentity();
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
            identity.AddClaim(new Claim(ClaimTypes.Email, email));
            identity.AddClaim(new Claim(ClaimTypes.Role, role));

            var controllerContext = new Mock<ControllerContext>();
            var principal = new Mock<IPrincipal>();
            principal.Setup(p => p.IsInRole(EnumUserRole.Admin)).Returns(true);
            principal.SetupGet(x => x.Identity.Name).Returns(userId.ToString());
            principal.Setup(x => x.Identity).Returns(identity);
            controllerContext.SetupGet(x => x.HttpContext.User).Returns(principal.Object);
            controllerContext.SetupGet(s => s.HttpContext.Session[It.IsAny<string>()]).Returns(-1);
            return controllerContext.Object;
        }*/
    }
}
