   M S B A M L   `   `   `  ����  GedcomViewCompare#    GedcomViewCompare.WMainWindow   5   	   \ XPresentationFramework, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   8x,http://schemas.microsoft.com/winfx/2006/xaml    5   	   >d2http://schemas.microsoft.com/expression/blend/2008    5   :   Hmc;http://schemas.openxmlformats.org/markup-compatibility/2006    6w   -   5          � Title$  GEDCOM View and Compare��5   	   $	��390��6�   $	��700��6�    � WindowStyle$ ThreeDBorderWindow=�6  ���	#FFE0E0EC
���   � � Icon$> 8/GedcomViewCompare;component/Images/GVC_Logo_100x100.png��6V  .��5      � ���	#00000000 � RowDefinitions 5   
   �� 5      -   5         �Name$ rowMenu��5      $��23 �6+   �� 5      -   5        $ rowInformation��5      $��20 �62   �� 5	      -   5        $ rowData��5	      $
��270* �6+   �� 5
      -   5        $ rowDecorator��5
      $��10 �60    � ColumnDefinitions 5   
   �� 5      -   5        $ columnDecorator��5      $��10 �66   �� 5      -   5        $ GridColumn1��5      �� 5      -   5        $ GridColumn2��5      �� 5      -	   5        $ GridColumn3��5      $��80 �62   �� 5      -
   5        $ columnDecorator2��5      $��10 �67   .+�5   
   o� -   5        $��menuFile��5   J   $��2��6   $��20��6#   $
��Left=�6/   $	��Top=�6Z   $
��Auto��6r   ���	#FFE0E0EC.@�5      m� $��_File��6   .@�5      m� -   5        $��_Open File(s)��5       m� IsCheckable .  6F   m� 5      -   5        $��_Save File(s) As...��5       .  6L   5      �� 5      6   m� 5      $��_Help��6   {�. .@�5      m� -   5        $��_About��5       .  {�. 6P   5      5      �� 5   
   -   5        $��textBoxGDI1��5   Q   $��1��6   $��2��6&   $��Stretch=�63   $��Stretch=�6d   $��1q�6�   " �� VerticalScrollBarVisibility$ Visible=�6�    �� 
IsReadOnly . 6�   �� 5   
   -   5        $��textBoxGDI2��5   Q   $��2��6   $��2��6&   $��Stretch=�63   $��Stretch=�6d   $��1q�6�   $ Visible=�6�    . 6�   �� 5   
   -   5        $��
headerGDI1��5   k   $��1��6   $��1��6!   $��Stretch=�61   $��Stretch=�6O   6k   �� 5    
   -   5        $��
headerGDI2��5    k   $��1��6   $��2��6!   $��Stretch=�61   $��Stretch=�6O   6k   �� 5$   
   -   5        $��
buttonSort��5$   }   $��Surname Sort��6   $��2��6(   $��3��65   $��23��6E   $
��Left=�6Q   $��0,0,0,0q�6l   $	��Top=�6�   $��75��6�   	 � ToolTip$3	 -Sort entries by surname of individual persons��6�   6�   �� 5%   
   -   5        $��buttonHighlight��5%   y   $��Compare��6   $��2��6#   $��3��60   $��23��6@   $
��Left=�6L   $��0,25,0,0q�6g   $	��Top=�6�   $��75��6�   $;	 5Highlight persons that are available only in one file��6�   6�   �� 5&   
   -   5        $��
buttonDiff��5&   |   $��
Difference��6   $��2��6&   $��3��63   $��23��6C   $
��Left=�6O   $��0,50,0,0q�6j   $	��Top=�6�   $��75��6�   $B	 <Create a new document with perons available in only one file��6�   {�.  6  �� 5'   
   -   5        $��buttonIntersection��5'   ~   $��Intersection��6   $��2��6(   $��3��65   $��23��6E   $
��Left=�6Q   $��0,75,0,0q�6l   $	��Top=�6�   $��75��6�   $^	 XJoin the two files and create a new document, duplicate entries will be converted to one��6�   {�.  6?  �� 5(   
   -   5        $��buttonUnion��5(   x   $��Union��6   $��2��6!   $��3��6.   $��23��6>   $
��Left=�6J   $��	0,100,0,0q�6e   $	��Top=�6�   $��75��6�   $\	 VJoin the two files and create a new document, with all persons available in both files��6�   {�.  6)  5)      5*      