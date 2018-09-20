#ifndef _KRUSTAL_H_
#define _KRUSTAL_H_

#include <vector>
#include <fstream>
#include <iostream>
#include <sstream>
#include <string>


struct edge
{
    int source;
    int destine;
    int weight;
};


class Krustal
{
    private:

        struct edge **_edge; // Use this to create the Conexions
        std::fstream file;
        std::vector<std::vector<int> > numbers;
        //std::vector<int> uniqueNum;
        int NVertices;
        int NEdges;
        int *NConj;
        static int QuickSort(edge *_edges, int start, int end);
        static int Partition(edge *_edges, int start, int end); 

    public:

        Krustal();
        static int GetInfoContent(Krustal *krustalInstance);
        static int OpenFile(Krustal *krustalInstance, const char *argv);
        static void PrintNumbers(Krustal *krustalInstance);
        static int NVertice(Krustal *krustalInstance);
        static int CreateEdges(Krustal *krustalInstance);
        static void PrintEdges(Krustal *krustalInstance);
        static void SortKrustal(Krustal *krustalInstance);
        
};



#endif