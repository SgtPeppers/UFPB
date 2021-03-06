#include <GL/glut.h>

double angTerra = 0;

void DisplayF();

int main(int argc, char *argv[])
{
    glutInit(&argc, argv);
    glutInitDisplayMode(GLUT_SINGLE);
    glutInitWindowSize(500, 500);
    glutCreateWindow("Solar System");
    glutDisplayFunc(DisplayF);
    glutIdleFunc(DisplayF); 
    /* code */
    
    glutMainLoop();

    return 0;
}

void DisplayF()
{
    glMatrixMode(GL_MODELVIEW);
    glLoadIdentity();
    glClearColor(0, 0, 0, 1);
    glClear(GL_COLOR_BUFFER_BIT);
    glOrtho(-70, 70, -70, 70, 1, 70);
    glTranslated(0, 0, -2);
    glEnableClientState(GL_VERTEX_ARRAY);
    glEnableClientState(GL_COLOR_ARRAY);
    gluLookAt(20, 30, 20, 0, 0, 0, 0, 0, 1);


    glColor3f(1, 0.25, 0);
    glTranslated(0, 0, 5);
    glutWireSphere(10, 500, 500);

    glRotated(angTerra, 0, 0, 1);
    glColor3d(0, 0, 1);
    glTranslated(-25, 0 , 5);
    glutWireSphere(6, 500, 500);

    glColor3d(1, 1, 1);
    glTranslated(-15, 0, 5);
    glutWireSphere(2, 500, 500);

    angTerra += 0.75;
    
    glFlush();
}

