# �u�ǉ��v�{�^���@�\����������

## �T�v
ListView�őI�����ꂽ�M����g�`�\�����X�g�iDragableList�j�ɒǉ�����@�\���������A`SampleSignals`�v���p�e�B��K�؂Ȗ��O�ɕύX���܂����B

## ���������ύX���e

### 1. ViewModel�̍X�V�iMainWindowViewModel.cs�j

#### �v���p�e�B���̕ύX�ƐV�K�ǉ�
```csharp
// ��: SampleSignals �� �V: SelectedSignalsForWaveform
/// <summary>
/// �g�`�\���p�ɑI�����ꂽ�M���̃R���N�V����
/// DragableList�ŕ\������A���[�U�[���g�`���m�F�������M���̃��X�g
/// </summary>
[ObservableProperty]
ObservableCollection<VariableDisplayItem> selectedSignalsForWaveform = new();

// �V�K�ǉ�: ListView�I�����ڊǗ�
/// <summary>
/// ListView �Ō��ݑI������Ă���M��
/// �u�ǉ��v�{�^���Ŕg�`�\�����X�g�ɒǉ�����Ώۂ̐M��
/// </summary>
[ObservableProperty]
VariableDisplayItem? selectedSignal;
```

#### �V�����R�}���h�̎���
```csharp
/// <summary>
/// �I�����ꂽ�M����g�`�\�����X�g�ɒǉ�����R�}���h
/// ListView�őI�����ꂽ�M����DragableList�ɒǉ�����
/// </summary>
[RelayCommand]
public void AddSelectedSignalToWaveform()
{
    if (SelectedSignal != null)
    {
        // �d���`�F�b�N�@�\�t��
        var existingSignal = SelectedSignalsForWaveform.FirstOrDefault(s => 
            s.Name == SelectedSignal.Name && 
            s.Type == SelectedSignal.Type && 
            s.BitWidth == SelectedSignal.BitWidth);

        if (existingSignal == null)
        {
            // �V�����C���X�^���X���쐬���Ēǉ�
            var newSignal = new VariableDisplayItem
            {
                Name = SelectedSignal.Name,
                Type = SelectedSignal.Type,
                BitWidth = SelectedSignal.BitWidth,
                VariableData = SelectedSignal.VariableData
            };
            SelectedSignalsForWaveform.Add(newSignal);
        }
        else
        {
            // �d���G���[�̃��[�U�[�ʒm
            MessageBox.Show($"�M�� '{SelectedSignal.Name}' �͊��ɔg�`�\�����X�g�ɒǉ�����Ă��܂��B", 
                "�d���G���[", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    else
    {
        // ���I���G���[�̃��[�U�[�ʒm
        MessageBox.Show("�ǉ�����M����I�����Ă��������B", 
            "�I���G���[", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
```

#### �e�X�g�R�}���h�̍X�V
```csharp
// �����̃e�X�g�R�}���h��V�����v���p�e�B���ɑΉ�
[RelayCommand]
public void ModifyFirstSignal() // SelectedSignalsForWaveform���g�p

[RelayCommand]
public void AddTestSignalToWaveform() // SelectedSignalsForWaveform�ɒǉ�

[RelayCommand]
public void RemoveLastSignalFromWaveform() // SelectedSignalsForWaveform����폜
```

#### �����f�[�^�̐ݒ�
```csharp
/// <summary>
/// �R���X�g���N�^ - �e�X�g�p�̏����f�[�^��ݒ�
/// </summary>
public MainWindowViewModel()
{
    // �f���p�̏����M���f�[�^��ݒ�
    SelectedSignalsForWaveform.Add(new VariableDisplayItem 
    { 
        Name = "clk", 
        Type = "Wire", 
        BitWidth = 1,
        VariableData = new Variable(Variable.VariableType.Wire, 1, "clk_id", "clk")
    });
    
    SelectedSignalsForWaveform.Add(new VariableDisplayItem 
    { 
        Name = "reset", 
        Type = "Wire", 
        BitWidth = 1,
        VariableData = new Variable(Variable.VariableType.Wire, 1, "reset_id", "reset")
    });
}
```

### 2. View�iMainWindow.xaml�j�̍X�V

#### ListView��SelectedItem�o�C���f�B���O�ǉ�
```xaml
<ListView
    Grid.Row="1"
    DisplayMemberPath="DisplayText"
    ItemsSource="{Binding ViewModel.SignalList}"
    SelectedItem="{Binding ViewModel.SelectedSignal}" />
```

#### �{�^���̃R�}���h�ƃe�L�X�g�X�V
```xaml
<Button
    Padding="20,5,20,5"
    HorizontalAlignment="Center"
    Background="{DynamicResource AccentFillColorDefaultBrush}"
    Command="{Binding ViewModel.AddSelectedSignalToWaveformCommand}"
    Content="�g�`�ɒǉ�"
    FontSize="16"
    Foreground="{DynamicResource TextOnAccentFillColorPrimaryBrush}" />
```

#### DragableList��ItemsSource�X�V
```xaml
<uc:DragableList
    Margin="5"
    x:Name="SignalDragableList"
    Grid.Row="1"
    Grid.Column="0"
    VerticalAlignment="Top"
    DisplayMemberPath="DisplayText"
    ItemsSource="{Binding ViewModel.SelectedSignalsForWaveform}" />
```

#### �e�X�g�p�{�^���̒ǉ�
```xaml
<StackPanel Grid.Row="0" Grid.Column="0" Orientation="Horizontal" Margin="5">
    <Button Command="{Binding ViewModel.ModifyFirstSignalCommand}" Content="First�ύX" Margin="2" Padding="5"/>
    <Button Command="{Binding ViewModel.AddTestSignalToWaveformCommand}" Content="�e�X�g�M���ǉ�" Margin="2" Padding="5"/>
    <Button Command="{Binding ViewModel.RemoveLastSignalFromWaveformCommand}" Content="�Ō�폜" Margin="2" Padding="5"/>
</StackPanel>
```

#### �g�`�\���G���A�̃v���[�X�z���_�[
```xaml
<TextBlock
    Grid.Row="1"
    Grid.Column="1"
    Text="�g�`�\���G���A�i��������j"
    HorizontalAlignment="Center"
    VerticalAlignment="Center"
    FontSize="16"
    Foreground="Gray" />
```

## ���������@�\�̏ڍ�

### 1. �M���ǉ��@�\
- **�I��**: ListView�ŐM����I��
- **�ǉ�**: �u�g�`�ɒǉ��v�{�^���N���b�N�őI��M����DragableList�ɒǉ�
- **�d���h�~**: �����M���̏d���ǉ����`�F�b�N���A���[�U�[�ɒʒm
- **�G���[�n���h�����O**: ���I�����̃G���[���b�Z�[�W�\��

### 2. �v���p�e�B���̉��P
- **��**: `SampleSignals` (�T���v���f�[�^�̂悤�Ȗ��O)
- **�V**: `SelectedSignalsForWaveform` (�p�r�����m�Ȗ��O)

### 3. ���[�U�r���e�B�̌���
- **�����I�ȑ���**: ListView�I�� �� �{�^���N���b�N �� DragableList�ɒǉ�
- **���o�I�t�B�[�h�o�b�N**: �G���[�_�C�A���O�ł̓K�؂Ȓʒm
- **�e�X�g�@�\**: ����m�F�p�̃e�X�g�{�^���Q

### 4. �A�[�L�e�N�`���̉��P
- **MVVM�p�^�[������**: View��ViewModel�̓K�؂ȕ���
- **�f�[�^�o�C���f�B���O**: �o�����o�C���f�B���O�ɂ���ԊǗ�
- **�R�}���h�p�^�[��**: RelayCommand�ɂ�鑀��̒��ۉ�

## ����t���[

1. **VCD�t�@�C���ǂݍ���**: ���W���[���c���[�ƐM�����X�g��\��
2. **���W���[���I��**: TreeView�Ń��W���[����I��
3. **�M���\��**: �I�����W���[���̐M����ListView�ɕ\��
4. **�M���I��**: ListView�Œǉ��������M����I��
5. **�g�`�ǉ�**: �u�g�`�ɒǉ��v�{�^�����N���b�N
6. **DragableList�\��**: �I�����ꂽ�M����DragableList�ɒǉ�
7. **���ёւ�**: DragableList�Ńh���b�O&�h���b�v�ɂ����ёւ����\

## �e�X�g�@�\

- **First�ύX**: �ŏ��̐M���̃v���p�e�B��ύX�i�v���p�e�B�ύX�ʒm�e�X�g�j
- **�e�X�g�M���ǉ�**: �����_���ȃ_�~�[�M����ǉ��i�R���N�V�����ύX�e�X�g�j
- **�Ō�폜**: �Ō�̐M�����폜�i�폜�@�\�e�X�g�j

## ����̊g���\��
- �g�`�\���G���A�̎���
- �M���̔g�`�f�[�^����
- ���Ԏ��i�r�Q�[�V�����@�\
- �M���l�̏ڍו\��

���̎����ɂ��AVCD�g�`�r���[�A�̊�{�I�ȐM���Ǘ��@�\���������A���[�U�[�������I�ɐM����I���E�Ǘ��ł���UI����������܂����B