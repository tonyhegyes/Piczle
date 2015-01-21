using System;
using _3DTools;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;


namespace puzzle
{
    
    public partial class rubikWindow : Window
    {
        int[] availableColors = new int[6] { 9, 9, 9, 9, 9, 9 };

        public rubikWindow()
        {
            InitializeComponent();

            for (int c = 0; c < 27; c++)
                if (c == 13) continue;
                else draw_Cube(c);

            Trackball tb = new Trackball();
            rubik_viewPort.Camera.Transform = tb.Transform;
            tb.EventSource = rubikCube_grid;
        }

        private void draw_Cube(int cube_nr)
        {
            MeshGeometry3D mesh;
            GeometryModel3D mGeometry;
            ContainerUIElement3D cube_Container = new ContainerUIElement3D() {  };
            
            //DRAW BACKGROUND
            mesh = new MeshGeometry3D();
            mesh.Positions.Add(new Point3D(0, 0, 0));

            for (int i = 0; i < 4; i++)
            {
                mesh.Positions.Add(new Point3D(cube_nr % 3 + (i % 2 == 0 ? 0.1 : 0.9), cube_nr / 3 - 3 * (cube_nr / 9) + (i < 2 ? 0.1 : 0.9), cube_nr / 9 + 0.1 ));
                mesh.Positions.Add(new Point3D(cube_nr % 3 + (i % 2 == 0 ? 0.1 : 0.9), cube_nr / 3 - 3 * (cube_nr / 9) + (i < 2 ? 0.1 : 0.9), cube_nr / 9 + 0.9));
            }
           
            //--FRONT
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(7);
            mesh.TriangleIndices.Add(7);
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(1);
            
            //--LEFT
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(6);
            mesh.TriangleIndices.Add(2);
           
            //--RIGHT
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(8);
            mesh.TriangleIndices.Add(8);
            mesh.TriangleIndices.Add(7);
            mesh.TriangleIndices.Add(3);
            
            //--BACK
            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(6);
            mesh.TriangleIndices.Add(6);
            mesh.TriangleIndices.Add(8);
            mesh.TriangleIndices.Add(4);
            
            //--UP
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(7);
            mesh.TriangleIndices.Add(8);
            mesh.TriangleIndices.Add(8);
            mesh.TriangleIndices.Add(6);
            mesh.TriangleIndices.Add(5);

            //--DOWN
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(2);
            
            //--ADD THE BACKGROUND 
            mGeometry = new GeometryModel3D(mesh, new DiffuseMaterial(Brushes.LightGray));
            mGeometry.Transform = new Transform3DGroup();
            cube_Container.Children.Add(new ModelUIElement3D() { Model = mGeometry });

        
            rubik_viewPort.Children.Add(cube_Container);
        }
    
        private void viewPort_Zoom(object sender, MouseWheelEventArgs e)
        {
            camera.Position = new Point3D(
                        camera.Position.X,
                        camera.Position.Y,
                        camera.Position.Z - e.Delta / 250D);
        }
    }
}
