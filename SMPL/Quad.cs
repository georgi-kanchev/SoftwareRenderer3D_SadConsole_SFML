using SadConsole;
using SFML.Graphics;

namespace SMPL
{
   public class Quad
   {
      public Area Area { get; } = new();
      private readonly Triangle[] triangles;
      public bool ShowBackside { get; set; } = true;

      public Quad(Image image)
      {
         triangles = new Triangle[]
         {
            new(new(new(-10, -10, 0), new(0, 0, 1)), new(new(-10, 10, 0) , new(0, 1, 1)), new(new(10, -10, 0), new(1, 0, 1)), image),
            new(new(new(-10, 10, 0), new(0, 1, 1)), new(new(10, 10, 0) , new(1, 1, 1)), new(new(10, -10, 0), new(1, 0, 1)), image)
         };
      }

      public void Draw(Console console, Camera camera)
      {
         for (int i = 0; i < 2; i++)
         {
            triangles[i].UpdatePoints(Area, console, camera);
            triangles[i].Draw(console, triangles[i].Image, ShowBackside == false);
         }
      }
   }
}
