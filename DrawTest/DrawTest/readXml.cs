using System;
using System.Xml.Linq;

namespace ReadMe {

	class Test {
		static void main(String[] args){
			XDocument doc = XDocument.Load("cal.xml");
			var authors = doc.Descendants( "double" );
			
			foreach ( var author in authors )
			{
				Console.WriteLine( author.Value );
			}
			Console.ReadLine();
		}
	}
}