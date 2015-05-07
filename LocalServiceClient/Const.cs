using System.Collections.Generic;
using System.Linq;

public static class Const
{
    static Const()
    {
        //0-9
		AllowedSymbols.AddRange(Chars(48, 57));
        //A-z
		AllowedSymbols.AddRange(Chars(65, 90));
		AllowedSymbols.AddRange(Chars(97, 122));
        //А-я
		AllowedSymbols.AddRange(Chars(1040, 1071));
		AllowedSymbols.AddRange(Chars(1072, 1103));
		//Symbols		
		AllowedSymbols.Add('.');
		AllowedSymbols.Add('-');
		AllowedSymbols.Add('#');
    }
    
	public static int MaxRetry = 10; 
    public static int MaxLenghtShort = 90;
    public static int MaxLenghtLong = 180;
    public static List<char> AllowedSymbols = new List<char>();
	
	public static string SpoilerScript = "if (this.parentNode.parentNode.getElementsByTagName('div')[1].getElementsByTagName('div')[0].style.display != '') { this.parentNode.parentNode.getElementsByTagName('div')[1].getElementsByTagName('div')[0].style.display = ''; this.innerText = ''; this.value = '...'; } else { this.parentNode.parentNode.getElementsByTagName('div')[1].getElementsByTagName('div')[0].style.display = 'none'; this.innerText = ''; this.value = '...'; }";
	
	public static char Ruble = (char)8381;
	
	public static int IconSize = 20;
	
	public static string AppStoreIcon = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAADIAAAAyCAYAAAAeP4ixAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAASrSURBVGhD1ZpfaFt1FMfPuQmsYFL34EMfKkzYQ5GBm/iwgmCGL30YuIcNERUdKFKoYE1aV9Be7ubDbJN1QkVERYcVK1S2BwWHwlZWcODAgj7soWAe9rAHH0YWsMXkHr+/5NB5m99N0oQmPz9Qcs7v5v755pzz+3fL1Ef8uYXD4lVfgZlh4e+Cqez79SO7x9PPnmIEzM4XbpAX/sbEb+HvMJqT9aOd0XMh/nzhXfHCX5npaW2qI3RPrY7oqZDZfH6BmM4hnxt/fU9uqdURPRPi5/M5k0bqRhCiTRoYWFe3I3oixJ+fPyTE59S1IF8GExNldTqiJ0KEPJNOA+pGMNFg5oK6HbPnQvyFhQOoi+PqNsAhzQTZ7Ia6HbP3EanImLW4AaKxEkxnL6rbFXsuRDh8Rs0IRgTfL72gbtf0oEZ4SI0aIlKGijfP5rKngiCoaHPXdDVFqeV/tTpMlUoxOHPmjjZHmM0XruHBD0FCEd3vJWJZDnK5v/RwDX9xMUVbW0MUhsOUTBaDycmiHmqbXQnxfX+AUg+/inR5A6eO7OyJRGiNWX4g5pVmBVy7Tjo9JsLPwT2OUf6R+pEHIPWus9AnVC5dQeQ2tTmWtoWYAQ03fsd2UxtCso6LfwvzNhRukOelYB+UkFAzchJd7v7aF1sAQXeYZBxR/F6brLQU4s/NDQl7X+DGY9rUFxDtz7hcGo+rq4R+WkEURiDiF4gws9O+gkx4UvbtezwzOnp5dXU11OZtYnstFPJ+pNJlhCzS6/QTPMtJSae/UjeCVQiKMSmV6jf4FUa0yQlQL3c59D5XN4I9Ig8NTvS7JnaC8ecmh9UjwfTbP2tThIZiN12jpAf/dCmlIKLIycQRjC+xi6/GiKTTLzklArM1pPjLzUQYGoTgxGfVdARZxBiypk4slhrhjBpOwGHikppNiQjxz58fdqw2NoLpybaWwNGIJJMH1HIEXlGjJVEhzE4JQXZsqdmSqBCRrjbJ+oml2P+fOC0EQ8ETarZkp5DIys0BotuqTYgKSST+UMsJzCIOS4m2xESEmLVybXPAIZrvUD7AViO39dMJ0AVnEJUT6sZiEcLWaXI/QVQ+NqtVda00CGEKr6rpDGbaBDHXmolpjEi5vOZanRhqYoRvQMxrZgWrzds0CNFdiit1zy3qW1H8qaQGf/cvXHhKm2vYip3Y44/UdBOmg5hORRZaViFBNnsT6dXVq7C9RRp2Mq1CDIjKh2o6BxZbH6i5TawQKpWWzbanes6AZ1q2LbZihZii59CbUtcJTG/KicSMuhHiIwLMHpIINd087iVMPBP3yqGpEAN7NOnCuIIp/fVgKruobgMthZjeAYU/rm5fqKeUd1pdKy2FGCBmCRdbUrfnIKVeb/UWqy0hBi7fP23Cq27vEHoPKbWsXiyYwrSPedcnf2/+xMxHtWlPQUeTPzuVbavnbDsiBvNvFojMMXMDbbKCNCzilwzwM41S5Z9Hg1yWzR8lvMdIwmMYCy7iGrHLapx/D9871a4Iw64i8l/8QuEobvg8zAwuY6bXG3iCdXQMX6Omfqx9qQm1GWwqdULIexEuzpdhzApvMclVqlSW4t4S2yH6F/Rnmc148JFiAAAAAElFTkSuQmCC";
    
	public static string GooglePlayIcon = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAADIAAAAyCAYAAAAeP4ixAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAARiSURBVGhD7ZpfaFt1FMfP+fWO9WEPLQhOKbMPmsyyB4U9+OCDQaT2Ybg2FRSEpDrEieAEQQXBDR/0TWWCyDbSi4XNSRq1AycITlTwwYeKxaZ/xA760LcVtknW5t7jOTcnf26TzfvHpAHzgfR3zkma3G/u+Z3f79wbiEsin5lLfP3iXeqGZuTiy/uSs9nz6kbG6BgdNJ/j9tYZ9UJT7vv7NAL8rO7ukpjNfJHIZ4+pG5hkfmqSz8Y36saCv4z4SGpheetHJPM0IA275I4C4mEEGuKP4IdA6wSwAYS/gqFvwYEif/qcC3tSq+mz65XXRCd+ajFWySoBQZHQ/Z2A5hDxFf6GHqmLEHAIgcUhvISEBTS4yLH1bbN9Q18Qi9hCEoXMUce6ucjf7lENBYZFPbaXcDExO/WchiITK7U4vz/g4UTFi82HSxPTr6kdmshnZKcITqm3XKSH2fpOQ3eA1nkupTjVjvD/VVPrhL5nJCIJSeazr/PQIAI2lifs91fG7XkE85GGbw/hzNK4faU4kbvE3uVK0EPEvK12KEKn1sjFzH6njycqwoCGPFyiZ7cMXO4n/ITdZyrR28IVi8bQ7dtH4HzPk6VxQS3xcw+y0DX1AxH6jDgWvLtThGAQz7OIa2z+mwjhIJ+Vv6TK7RAh9PNz76kdmNBCOI2CHGgsiGhSti7qBiKUkAcKmYd4gob6gCjwOmQ5e24eVjcQoeaIrBng4qvqthWuZp+upO0L6v5/aHlG7itkBvaSOWT1mY0/njq3qmFIFjLDPMij7ZDDJX3SLqoLI1+9cL/jlodKAPNXx+1NDddoEiLpgy7m6pWJzu4fHD7+Q+pUOZnPnOQEfqcSby9EML2cnp4Sm3ue07J/8+LeAmqOL0/kZsSv4pvsXoNE8Jm/vOKxjWtrobfo/xWcBdmqCEGKDe+qz2h21PBXLefWoy2rEuGoWp3HhcfVaqTfdWV3XccnhPOsaaETeO1oGe8E5GsF6hjDC2cDoRfEbqUnpNvoCek2ekK6jZ6QbqMnpFMgUlnNO+ITQmRqvUcj/GYt450BF9Twwfs/X0/iE3LP4IFfeKg1M1WQ4JyaHcdFson8Z4V7lbVbm77rYX4h0jw5ZD3Br5zhf17l8Se5GlhM2yJwV5CLfpwpY3IsIoBDF9BQ6uqUzc1inVAXH3arQwxCr2p1G+GEIAYqhbtBKCE88efVbDuI8KeagQgl5O7BA1LyfGWvLRDMl5A+Vi8QoapWlYP55w+RcSpX0V0c5Xd507Mb4YMBQ/47UIRv8N8nK04dqVBcUm2xXV7o7h0YXpClwHsyIJGENCLXnfgAc+rW4IO7wuUzpa5HIp/Nccpk1a1DdGopbZ9ULxK98ttt9IR0Gz0hVVzHtPwdCcq99CboNzV87OwtohBbyNZ1l7fXzY2XS8ZbFxpBA182/EDAQ3yJqxuZ2EKkL7Asa4zFXJKD4seC3HNfmcw1/QJC7p3zOnJEegtWsOmNYMbC3lNvBuAfkUWJSMhwF08AAAAASUVORK5CYII=";
	
	public static string WindowsStoreIcon = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAADIAAAAyCAYAAAAeP4ixAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAICSURBVGhD7Zq9SgNBEIBnzogRUqSwsBBykMP8ELG1kxSigqWvYO8LyCadvU9gb6cosTkfwMLKv0asFCtLTfDG2b2NFkaTGJObyH6w7F5uivl29wJ3OwhjRE6F6WloBggU8GVAiHnufT0WJ7KowmwLmia5iBNGhDwBFAHQ52Tn4qivJCIyr8IZD1rtmfUJqcCJmFnmlGZMUJ8MVaSoTpe4qyASbwEys8yzywKYNQF/yEAiZXU0CzClt0BRJ/tGtH9bX7u2t6FUa9xxpwWGTleRgjrxPbvshJ5+uMyW4JnlPYsZE2SJKKre1NfP7OVoRZZVmHqA18Am62P8TxAnrh82wJQO7IUkRbwnbLYmEK9Y4JDbHv+2zW2DW7EfiaThhfgfOBFpOBFpOBFpOBFpOBFpOBFpOBFpoH4DtOOBeYH04329+mIvYUEdzzXH6J3G4egAlmoNsuOBSfTjg+3HHiciDSciDSciDSciDSciDSciDSciDQ8oWiHCLR7vEtABAJ1z/xzfHh++PZ7W1QkILV8fRZuqBKRct1KKRI+nbd8X7eKWdjWDPdL2kWDnsr56YYIY8SK9UlaNTV7FCiHlzaqa9rtak24MVaQTZRVmwKymKUjwbalSvH0HWL2Ri/xEuwojxUKRlvuowoi3MKebNoEdECXSDf3l8g0mWTIu4vmsOILgHUJdmmfHZDYuAAAAAElFTkSuQmCC";
	
	private static IEnumerable<char> Chars(int start, int end)
	{
		var range = Enumerable.Range(start, end - start + 1);
		return range.Select(i => (char) i);
	}
}