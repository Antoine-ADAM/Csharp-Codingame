﻿using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public class BinaryNode
{
    public bool IsEndWord = false;
    public BinaryNode point;// true ('.')
    public BinaryNode midleDash;// false ('-')
}
public class Solution
{
    
    // '.' is true and '-' is false
    private static Dictionary<char, bool[]> morseBinary = new Dictionary<char, bool[]>
    {
        {'A', new []{true, false}},
        {'B', new []{false, true, true, true}},
        {'C', new []{false, true, false, true}},
        {'D', new []{false, true, true}},
        {'E', new []{true}},
        {'F', new []{true, true, false, true}},
        {'G', new []{false, false, true}},
        {'H', new []{true, true, true, true}},
        {'I', new []{true, false}},
        {'J', new []{true, false, false, false}},
        {'K', new []{false, true, false}},
        {'L', new []{true, false, true, true}},
        {'M', new []{false, false}},
        {'N', new []{false, true}},
        {'O', new []{false, false, false}},
        {'P', new []{true, false, false, false}},
        {'Q', new []{false, false, true, false}},
        {'R', new []{false, true, false}},
        {'S', new []{false, false, false, false}},
        {'T', new []{false}},
        {'U', new []{true, false, false}},
        {'V', new []{true, false, true, false}},
        {'W', new []{true, false, false, true}},
        {'X', new []{false, true, true, false}},
        {'Y', new []{false, true, false, false}},
        {'Z', new []{false, false, true, true}}
    };
    static void addWordToTree(string word, BinaryNode root)
    {
        BinaryNode current = root;
        foreach (char c in word)
            foreach (bool b in morseBinary[c])
            {
                if (b)
                {
                    if (current.point == null)
                        current.point = new BinaryNode();
                    current = current.point;
                }
                else
                {
                    if (current.midleDash == null)
                        current.midleDash = new BinaryNode();
                    current = current.midleDash;
                }
            }
        current.IsEndWord = true;
    }
    
    public static bool[] stringToBinary(string s)
    {
        bool[] result = new bool[s.Length];
        for (int i = 0; i < s.Length; i++)
            result[i] = s[i] == '.';
        return result;
    }

    public static void recFindWords(int index, BinaryNode node, bool[] morseBin, Dictionary<int, List<short>> indexWordsLength, int startIndex, int lengthMorseBin)
    { 
        if(node.IsEndWord)
        {
            if (!indexWordsLength.ContainsKey(index))
                indexWordsLength.Add(index, new List<short>{(short)(index - startIndex + 1)});
            else
                indexWordsLength[index].Add((short)(index - startIndex + 1));
        }
        if (node.point != null && index < lengthMorseBin && morseBin[index])
            recFindWords(index + 1, node.point, morseBin, indexWordsLength, startIndex, lengthMorseBin);
        else if (node.midleDash != null && index < lengthMorseBin && !morseBin[index])
            recFindWords(index + 1, node.midleDash, morseBin, indexWordsLength, startIndex, lengthMorseBin);
    }

    public static void Main(string[] args)
    {
        bool[] morseBin = stringToBinary(Console.ReadLine());
        int lengthMorseBin = morseBin.Length;
        int N = int.Parse(Console.ReadLine());
        BinaryNode dictionary = new BinaryNode();
        string[] debug = new string[N];
        for (int i = 0; i < N; i++)
        {
            string word = Console.ReadLine();
            addWordToTree(word, dictionary);
            debug[i] = word;
        }
            
        Dictionary<int,List<short>> indexWordsLength = new Dictionary<int,List<short>>();
        for (int i = 0; i < morseBin.Length; i++)
            recFindWords(i, dictionary, morseBin, indexWordsLength, i, lengthMorseBin);
        UInt64?[] combination = new UInt64?[lengthMorseBin];
        UInt64 res = recGetCombination(0, lengthMorseBin, combination, indexWordsLength);
        Console.WriteLine(res);
    }

    public static UInt64 recGetCombination(int index, int length, UInt64?[] combination, Dictionary<int,List<short>> indexWordsLength)
    {
        if (index == length)
            return 1;
        if (combination[index] != null)
            return (UInt64)combination[index];
        UInt64 count = 0;
        if (indexWordsLength.ContainsKey(index))
            foreach (short wordLength in indexWordsLength[index])
                count += recGetCombination(index + wordLength, length, combination, indexWordsLength);
        combination[index] = count;
        return count;
    }
}