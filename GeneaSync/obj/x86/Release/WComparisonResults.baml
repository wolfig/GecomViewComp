   M S B A M L   `   `   `  ����   GedcomViewCompare)    #GedcomViewCompare.comparisonResults   \ XPresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    8x,http://schemas.microsoft.com/winfx/2006/xaml    -     � Title$  Comparison Results��$	��146��$	��352��.��� $	��300��$��95�� � RowDefinitions �� -    �Name$ columnHeaders��$��15 ��� -   $ columnMissing��$��20 ��� -   $ columnNameMatch��$��20 ��� -   $ columnAmbigious��$��20 ��� -   $ columnMatch��$��20 � � ColumnDefinitions �� -   $ columnColorCode��$��80 ��� -   $ columnMatchStatus��$��20 ��� -	   $ columnResutsFile1��$	��100 ��� -
   $ columnResultsFile2��$	��100 �.+��� -   $��
textBlock0��$��0��$��0��$��Center=�$��Status��$��Center=� �� TextAlignment$ Center=�$
��Bold"��� -   $��
textBlock1��$��2��$��0��$��Center=�$��# in File 1��$��Center=�$ Center=�$
��Bold"��� -   $��
textBlock2��$��3��$��0��$��Center=�$��# in File 2��$��Center=�$ Center=�$
��Bold"��� -   $��
textBlock3��$��0��$��1��$��Center=�$��Match:��$��Center=�$ Center=� � ToolTip$$ Entries matching in both files���� -   $��
textBlock4��$��0��$��2��$��Center=�$��Missing:��$��Center=�$ Center=�$D >Entries which are only available in file where number is shown���� -   $��
textBlock5��$��0��$��3��$��Center=�$��Name-Match:��$��Center=�$ Center=�$; 5Entries where only the name (given + surname) matches���� -   $��
textBlock6��$��0��$��4��$��Center=�$��
Ambigious:��$��Center=�$ Center=�$V PEntries where more than one person with the same name is available in other file���� -   $��textBlock_Matc1��$��2��$��1��$��Center=�$��Center=�$ Center=��� -   $��textBlock_Miss1��$��2��$��2��$��Center=�$��Center=�$ Center=��� -   $��textBlock_NaMa1��$��2��$��3��$��Center=�$��Center=�$ Center=��� -   $��textBlock_Ambi1��$��2��$��4��$��Center=�$��Center=�$ Center=��� -   $��textBlock_Matc2��$��3��$��1��$��Center=�$��Center=�$ Center=��� -   $��textBlock_Miss2��$��3��$��2��$��Center=�$��Center=�$ Center=��� -   $��textBlock_NaMa2��$��3��$��3��$��Center=�$��Center=�$ Center=��� -   $��textBlock_Ambi2��$��3��$��4��$��Center=�$��Center=�$ Center=��� -   $��
rectangle0��$��1��$��1��$��20��$
��Left=�
���   �$��Center=�$��20���� -   $��
rectangle1��$��1��$��2��$��20��$
��Left=�
���   �$��Center=�$��20��
��������� -   $��
rectangle2��$��1��$��3��$��20��$
��Left=�
���   �$��Center=�$��20��
��� ����� -   $��
rectangle3��$��1��$��4��$��20��$
��Left=�
���   �$��Center=�$��20��
����·�